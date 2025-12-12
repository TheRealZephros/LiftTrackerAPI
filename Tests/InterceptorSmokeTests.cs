using System;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Tests.Helpers.Infrastructure.Auditing;

public class InterceptorSmokeTests
{
    [Fact]
    public async Task SmokeTest_AddUser_ProducesAudit()
    {
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = AuditInterceptorTestHelpers.CreateSampleUser("smoketest", "smoke@example.com");

        appDb.Users.Add(user);
        await appDb.SaveChangesAsync();

        var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);

        Assert.NotNull(audit);
        Assert.Equal("User", audit.EntityName);
        Assert.Equal("ADDED", audit.Action.ToUpperInvariant());
    }

    [Fact]
    public async Task SmokeTest_ModifyUser_ProducesAudit()
    {
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = AuditInterceptorTestHelpers.CreateSampleUser("before", "before@example.com");
        appDb.Users.Add(user);
        await appDb.SaveChangesAsync();

        user.UserName = "after";
        await appDb.SaveChangesAsync();

        var audit = await auditDb.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(audit);
        Assert.Equal("MODIFIED", audit.Action.ToUpperInvariant());
        Assert.Contains("UserName", audit.ChangedProperties);
    }

    [Fact]
    public async Task SmokeTest_SoftDelete_ProducesAudit()
    {
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = AuditInterceptorTestHelpers.CreateSampleUser("toDelete", "delete@example.com");
        appDb.Users.Add(user);
        await appDb.SaveChangesAsync();

        appDb.Users.Remove(user);
        await appDb.SaveChangesAsync();

        var audit = await auditDb.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(audit);
        Assert.Equal("DELETED", audit.Action.ToUpperInvariant());

        var deletedUser = await appDb.Users.IgnoreQueryFilters()
            .FirstAsync(x => x.Id == user.Id);

        Assert.True(deletedUser.IsDeleted);
    }
}
