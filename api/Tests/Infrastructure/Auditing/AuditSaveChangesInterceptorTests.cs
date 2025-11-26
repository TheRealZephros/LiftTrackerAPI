using System;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Infrastructure.Auditing;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

using static api.Tests.Helpers.Infrastructure.Auditing.AuditInterceptorTestHelpers;

namespace api.Tests.Infrastructure.Auditing
{
    public class AuditSaveChangesInterceptorTests
    {
        #region CREATE

        [Fact]
        public async Task SavingAddedEntity_CreatesAuditLog()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);
            var exercise = CreateSampleExercise();

            // Act
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            // Assert
            var audit = auditDb.AuditLogs.FirstOrDefault();
            Assert.NotNull(audit);
            Assert.Equal("Exercise", audit.EntityName);
            Assert.Equal(exercise.Id.ToString(), audit.EntityId);
            Assert.Equal("ADDED", audit.Action);
            Assert.Contains("Name", audit.NewValues);
            Assert.Contains("Description", audit.NewValues);
            Assert.Equal("test-correlation-id", audit.CorrelationId);
            Assert.Equal("127.0.0.1", audit.IpAddress);
            Assert.Equal("UnitTestAgent/1.0", audit.UserAgent);
        }

        #endregion

        #region UPDATE

        [Fact]
        public async Task UpdatingEntity_CreatesAuditLogWithDiff()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);
            var exercise = CreateSampleExercise();
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            // Modify entity
            exercise.Name = "Updated Name";

            // Act
            context.Exercises.Update(exercise);
            await context.SaveChangesAsync();

            // Assert
            var audit = auditDb.AuditLogs.Last();
            Assert.NotNull(audit);
            Assert.Equal("MODIFIED", audit.Action);
            Assert.Contains("Name", audit.ChangedProperties);
            Assert.Contains("Sample Exercise", audit.OldValues);
            Assert.Contains("Updated Name", audit.NewValues);
        }

        #endregion

        #region IGNORED PROPERTIES

        [Fact]
        public async Task UpdatingIgnoredProperty_DoesNotCreateAuditLog()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            var exercise = CreateSampleExercise();
            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            // Update an ignored property (simulate)
            var entry = context.Entry(exercise);
            entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;

            // Act
            await context.SaveChangesAsync();

            // Assert
            Assert.Empty(auditDb.AuditLogs.Skip(1)); // Only the first ADD log should exist
        }

        #endregion

        #region MULTIPLE CHANGES

        [Fact]
        public async Task MultipleEntities_Changes_CreateMultipleAuditLogs()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            var exercise1 = CreateSampleExercise("Ex1");
            var exercise2 = CreateSampleExercise("Ex2");

            context.Exercises.Add(exercise1);
            context.Exercises.Add(exercise2);
            await context.SaveChangesAsync();

            // Modify both
            exercise1.Name = "Ex1 Updated";
            exercise2.Description = "New description";

            // Act
            await context.SaveChangesAsync();

            // Assert
            var audits = auditDb.AuditLogs.Where(a => a.Action == "MODIFIED").ToList();
            Assert.Equal(2, audits.Count);
            Assert.Contains(audits, a => a.EntityId == exercise1.Id.ToString() && a.NewValues.Contains("Ex1 Updated"));
            Assert.Contains(audits, a => a.EntityId == exercise2.Id.ToString() && a.NewValues.Contains("New description"));
        }

        #endregion
        
        #region COMPLEX UPDATE DIFF

        [Fact]
        public async Task UpdatingMultipleProperties_CreatesCorrectAuditDiff()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            var exercise = new Exercise
            {
                Name = "Original Name",
                Description = "Original Description",
                IsUsermade = true,
                UserId = "user-123",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            // Act - change multiple properties
            exercise.Name = "Updated Name";
            exercise.Description = "Updated Description";
            exercise.IsUsermade = false;
            exercise.UserId = null; // nullable change
            exercise.CreatedAt = exercise.CreatedAt.AddHours(2);

            context.Exercises.Update(exercise);
            await context.SaveChangesAsync();

            // Assert
            var audit = auditDb.AuditLogs.Last();
            Assert.NotNull(audit);
            Assert.Equal("MODIFIED", audit.Action);

            // ChangedProperties should include all modified props
            Assert.Contains("Name", audit.ChangedProperties);
            Assert.Contains("Description", audit.ChangedProperties);
            Assert.Contains("IsUsermade", audit.ChangedProperties);
            Assert.Contains("UserId", audit.ChangedProperties);
            Assert.Contains("CreatedAt", audit.ChangedProperties);

            // OldValues JSON contains original values
            Assert.Contains("Original Name", audit.OldValues);
            Assert.Contains("Original Description", audit.OldValues);
            Assert.Contains("true", audit.OldValues);
            Assert.Contains("user-123", audit.OldValues);

            // NewValues JSON contains updated values
            Assert.Contains("Updated Name", audit.NewValues);
            Assert.Contains("Updated Description", audit.NewValues);
            Assert.Contains("false", audit.NewValues);
            Assert.Contains("null", audit.NewValues);
        }

        #endregion

        #region BATCH ENTITY UPDATES

        [Fact]
        public async Task UpdatingMultipleEntities_CreatesAuditLogsForAll()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            // Add initial entities
            var exercise1 = new Exercise
            {
                Name = "Exercise 1",
                Description = "Desc 1",
                IsUsermade = true,
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var exercise2 = new Exercise
            {
                Name = "Exercise 2",
                Description = "Desc 2",
                IsUsermade = true,
                UserId = "user-2",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            context.Exercises.AddRange(exercise1, exercise2);
            await context.SaveChangesAsync();

            // Act - modify both entities in one SaveChanges
            exercise1.Description = "Updated Desc 1";
            exercise2.IsUsermade = false;
            exercise2.UserId = null;

            await context.SaveChangesAsync();

            // Assert - check that both changes are logged
            var audits = auditDb.AuditLogs.OrderBy(a => a.Id).ToList();
            Assert.Equal(2, audits.Count);

            var audit1 = audits.First(a => a.EntityId == exercise1.Id.ToString());
            var audit2 = audits.First(a => a.EntityId == exercise2.Id.ToString());

            Assert.Equal("MODIFIED", audit1.Action);
            Assert.Contains("Description", audit1.ChangedProperties);
            Assert.Contains("Desc 1", audit1.OldValues);
            Assert.Contains("Updated Desc 1", audit1.NewValues);

            Assert.Equal("MODIFIED", audit2.Action);
            Assert.Contains("IsUsermade", audit2.ChangedProperties);
            Assert.Contains("UserId", audit2.ChangedProperties);
            Assert.Contains("true", audit2.OldValues);
            Assert.Contains("user-2", audit2.OldValues);
            Assert.Contains("false", audit2.NewValues);
            Assert.Contains("null", audit2.NewValues);
        }

        #endregion

        #region SOFT DELETE USER

        [Fact]
        public async Task DeletingUser_SetsIsDeletedAndCreatesAuditLog()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            // Add a user to the context
            var user = new User
            {
                UserName = "softdeleteuser",
                Email = "softdeleteuser@test.com",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act - delete the user (soft delete)
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            // Assert - user is not physically deleted
            var dbUser = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(dbUser);
            Assert.True(dbUser.IsDeleted);
            Assert.NotNull(dbUser.DeletedAt);

            // Assert - audit log created
            var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);
            Assert.NotNull(audit);
            Assert.Equal("MODIFIED", audit.Action);
            Assert.Contains("IsDeleted", audit.ChangedProperties);
            Assert.Contains("DeletedAt", audit.ChangedProperties);
        }

        #endregion

        #region AUDIT USER UPDATES

        [Fact]
        public async Task UpdatingUser_TracksChangedPropertiesInAuditLog()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            // Add a user
            var user = new User
            {
                UserName = "updateuser",
                Email = "updateuser@test.com",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act - update some properties
            user.UserName = "updateduser";
            user.Email = "updateduser@test.com";
            await context.SaveChangesAsync();

            // Assert - audit log created
            var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);
            Assert.NotNull(audit);
            Assert.Equal("MODIFIED", audit.Action);

            // Check changed properties
            var changedProps = System.Text.Json.JsonSerializer.Deserialize<string[]>(audit.ChangedProperties);
            Assert.Contains("UserName", changedProps!);
            Assert.Contains("Email", changedProps!);

            // Check old/new values
            Assert.Contains("updateuser", audit.OldValues!);
            Assert.Contains("updateduser", audit.NewValues!);
            Assert.Contains("updateuser@test.com", audit.OldValues!);
            Assert.Contains("updateduser@test.com", audit.NewValues!);
        }

        [Fact]
        public async Task UpdatingUser_IgnoresPasswordHashAndTokensInAuditLog()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            // Add a user
            var user = new User
            {
                UserName = "ignoreuser",
                Email = "ignoreuser@test.com",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act - modify ignored properties
            var entry = context.Entry(user);
            entry.Property("PasswordHash").CurrentValue = "newhash";
            entry.Property("ConcurrencyStamp").CurrentValue = "newstamp";
            await context.SaveChangesAsync();

            // Assert - audit log does NOT include ignored properties
            var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);
            Assert.Null(audit); // no meaningful changes recorded
        }

        [Fact]
        public async Task AddingUser_CreatesAuditLogWithNewValues()
        {
            // Arrange
            var auditDb = CreateAuditDbContext();
            var interceptor = new AuditSaveChangesInterceptor(
                MockCorrelation().Object,
                MockUserContext().Object,
                MockHttpContextInfo().Object,
                auditDb
            );

            var context = CreateAppDbContext(interceptor);

            // Act - add a new user
            var user = new User
            {
                UserName = "newuser",
                Email = "newuser@test.com",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert - audit log created
            var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);
            Assert.NotNull(audit);
            Assert.Equal("ADDED", audit.Action);
            Assert.Contains("newuser", audit.NewValues!);
            Assert.Contains("newuser@test.com", audit.NewValues!);
        }

        #endregion


    }
}
