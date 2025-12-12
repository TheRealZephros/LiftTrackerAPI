using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Tests.Helpers.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using api.Infrastructure.Auditing;
using api.Interfaces;

namespace Tests.Infrastructure.Auditing
{
    public class AuditSaveChangesInterceptorTests
    {
        #region Helpers

        private static async Task<User> AddUser(ApplicationDbContext db)
        {
            var user = AuditInterceptorTestHelpers.CreateSampleUser("x", "x@example.com");
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        private static Task<AuditLog?> LatestAudit(AuditDbContext audit, string id)
        {
            return audit.AuditLogs
                .Where(a => a.EntityId == id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Spy Interceptor

        private class SpyAuditInterceptor : AuditSaveChangesInterceptor
        {
            public bool WasCalledBefore { get; private set; }
            public bool WasCalledAfter { get; private set; }

            public SpyAuditInterceptor(
                ICorrelationIdAccessor cid,
                IUserContext user,
                IHttpContextInfo http,
                AuditDbContext auditDb
            ) : base(cid, user, http, auditDb) { }

            public override InterceptionResult<int> SavingChanges(
                DbContextEventData eventData,
                InterceptionResult<int> result)
            {
                WasCalledBefore = true;
                return base.SavingChanges(eventData, result);
            }

            public override int SavedChanges(
                SaveChangesCompletedEventData eventData,
                int result)
            {
                WasCalledAfter = true;
                return base.SavedChanges(eventData, result);
            }

            public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
                DbContextEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                WasCalledBefore = true;
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            public override ValueTask<int> SavedChangesAsync(
                SaveChangesCompletedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                WasCalledAfter = true;
                return base.SavedChangesAsync(eventData, result, cancellationToken);
            }
        }

        #endregion

        #region Interceptor Registration Tests

        [Fact]
        public void Interceptor_Is_Registered_On_DbContext()
        {
            var (db, _, interceptor) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var options = db.GetService<IDbContextOptions>();

            var coreExt = options.Extensions
                .OfType<CoreOptionsExtension>()
                .FirstOrDefault();

            Assert.NotNull(coreExt);
            Assert.Contains(interceptor, coreExt.Interceptors);
            Assert.True(coreExt.Interceptors.Any(i => i is AuditSaveChangesInterceptor));
        }

        [Fact]
        public void Ensure_Exactly_One_AuditInterceptor_Is_Registered()
        {
            var (db, _, interceptor) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var options = db.GetService<IDbContextOptions>();

            var coreExt = options.Extensions
                .OfType<CoreOptionsExtension>()
                .FirstOrDefault();

            Assert.NotNull(coreExt);

            var auditInterceptors = coreExt.Interceptors
                .Where(i => i is AuditSaveChangesInterceptor)
                .ToList();

            Assert.Single(auditInterceptors);
            Assert.Equal(interceptor, auditInterceptors[0]);
        }

        #endregion

        #region Interceptor Invocation Tests

        [Fact]
        public async Task Interceptor_Is_Invoked_On_SaveChanges()
        {
            var dbName = Guid.NewGuid().ToString();
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var auditOptions = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(dbName)
                .UseInternalServiceProvider(provider)
                .Options;

            using var auditDb = new AuditDbContext(auditOptions);
            var interceptor = new SpyAuditInterceptor(
                AuditInterceptorTestHelpers.MockCorrelationIdAccessor().Object,
                AuditInterceptorTestHelpers.MockUserContext().Object,
                AuditInterceptorTestHelpers.MockHttpContextInfo().Object,
                auditDb
            );

            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .UseInternalServiceProvider(provider)
                .AddInterceptors(interceptor)
                .Options;

            using var appDb = new ApplicationDbContext(appOptions);

            var user = AuditInterceptorTestHelpers.CreateSampleUser("spyUser", "spy@example.com");

            appDb.Users.Add(user);
            await appDb.SaveChangesAsync();

            Assert.True(interceptor.WasCalledBefore);
            Assert.True(interceptor.WasCalledAfter);
        }

        #endregion

        #region Audit Behavior Tests

        [Fact]
        public async Task AddingUser_CreatesAuditLog()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var user = await AddUser(app);

            var auditLog = await LatestAudit(audit, user.Id);

            Assert.NotNull(auditLog);
            Assert.Equal("ADDED", auditLog!.Action);
        }

        [Fact]
        public async Task ModifyingUser_CreatesAuditDiff()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var user = await AddUser(app);

            user.UserName = "updated";
            await app.SaveChangesAsync();

            var auditLog = await LatestAudit(audit, user.Id);

            Assert.NotNull(auditLog);
            Assert.Equal("MODIFIED", auditLog!.Action);
            Assert.Contains("UserName", auditLog.ChangedProperties);
        }

        [Fact]
        public async Task SoftDelete_CreatesAuditLog()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var user = await AddUser(app);

            app.Users.Remove(user);
            await app.SaveChangesAsync();

            var deletedUser = await app.Users
                .IgnoreQueryFilters()
                .FirstAsync(u => u.Id == user.Id);

            Assert.True(deletedUser.IsDeleted);

            var auditLog = await LatestAudit(audit, user.Id);

            Assert.NotNull(auditLog);
            Assert.Equal("DELETED", auditLog!.Action);
        }

        #endregion

        #region Negative Tests

        [Fact]
        public async Task No_Changes_No_AuditLog_Created()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var user = await AddUser(app);

            // No modification, no deletion, no nothing.
            await app.SaveChangesAsync();

            var audits = await audit.AuditLogs
                .Where(a => a.EntityId == user.Id)
                .ToListAsync();

            // Only one audit record should exist: the ADDED record.
            Assert.Single(audits);
            Assert.Equal("ADDED", audits[0].Action);
        }

        [Fact]
        public async Task Modifying_Untracked_Property_Does_Not_Create_AuditLog()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var user = await AddUser(app);

            // Modify a property EF Core does NOT track (e.g., a computed property or ignored field).
            // To emulate this, we clear modified markers manually.
            user.ConcurrencyStamp = "x-stamp"; // Identity fields are often ignored.

            // Force EF to believe nothing changed.
            app.Entry(user).State = EntityState.Unchanged;

            await app.SaveChangesAsync();

            var audits = await audit.AuditLogs
                .Where(a => a.EntityId == user.Id)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            // Only the ADDED audit should exist
            Assert.Single(audits);
            Assert.Equal("ADDED", audits[0].Action);
        }

        #endregion

        #region Multi-Entity Tests

        [Fact]
        public async Task Multiple_Entities_Modified_Produces_Multiple_AuditLogs()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var u1 = await AddUser(app);
            var u2 = await AddUser(app);

            u1.UserName = "u1-new";
            u2.UserName = "u2-new";

            await app.SaveChangesAsync();

            var logs = await audit.AuditLogs
                .Where(a => a.Action == "MODIFIED")
                .OrderBy(a => a.EntityId)
                .ToListAsync();

            Assert.Equal(2, logs.Count);

            Assert.Contains(logs, a => a.EntityId == u1.Id);
            Assert.Contains(logs, a => a.EntityId == u2.Id);

            Assert.All(logs, a => Assert.Contains("UserName", a.ChangedProperties));
        }

        [Fact]
        public async Task Multiple_Entity_Operations_Create_Mixed_AuditLogs()
        {
            var (app, audit, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var added = AuditInterceptorTestHelpers.CreateSampleUser("new", "n@example.com");
            app.Users.Add(added);

            var modified = await AddUser(app);
            modified.Email = "updated@example.com";

            var deleted = await AddUser(app);
            app.Users.Remove(deleted);

            await app.SaveChangesAsync();

            var logs = await audit.AuditLogs
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            Assert.Equal(5, logs.Count);

            Assert.Contains(logs, a => a.EntityId == added.Id && a.Action == "ADDED");
            Assert.Contains(logs, a => a.EntityId == modified.Id && a.Action == "MODIFIED");
            Assert.Contains(logs, a => a.EntityId == deleted.Id && a.Action == "DELETED");
        }

        #endregion
    }
}
