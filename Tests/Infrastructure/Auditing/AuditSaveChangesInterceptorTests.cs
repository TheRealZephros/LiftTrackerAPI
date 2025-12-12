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

        // ------------------------------------------------------------
        // NEW TEST: Assert Interceptor Is Actually Attached
        // ------------------------------------------------------------
       [Fact]
        public void Interceptor_Is_Registered_On_DbContext()
        {
            var (db, _, interceptor) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

            var options = db.GetService<IDbContextOptions>();

            var extension = options.Extensions
                .OfType<CoreOptionsExtension>()
                .FirstOrDefault();

            Assert.NotNull(extension);

            Assert.Contains(interceptor, extension.Interceptors);
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
