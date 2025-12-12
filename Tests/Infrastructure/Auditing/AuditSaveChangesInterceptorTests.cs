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
using api.Data;
using api.Infrastructure.Auditing;
using api.Interfaces;


namespace Tests.Infrastructure.Auditing
{
    public class AuditSaveChangesInterceptorTests
    {
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

            public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
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


        // Assert Interceptor Is Actually Attached
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


        // ------------------------------------------------------------

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

            var deletedUser = await app.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user.Id);
            Assert.True(deletedUser.IsDeleted);

            var auditLog = await LatestAudit(audit, user.Id);
            Assert.NotNull(auditLog);
            Assert.Equal("DELETED", auditLog!.Action);
        }
    }
}
