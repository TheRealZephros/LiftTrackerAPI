using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers.Infrastructure.Auditing;
using Xunit;

public class InterceptorSmokeTests
{
    #region Helpers

    private static async Task<AuditLog?> LatestAuditAsync(AuditDbContext auditDb)
    {
        return await auditDb.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private static async Task<User> AddUserAsync(ApplicationDbContext appDb, string name, string email)
    {
        var user = AuditInterceptorTestHelpers.CreateSampleUser(name, email);
        appDb.Users.Add(user);
        await appDb.SaveChangesAsync();
        return user;
    }

    #endregion

    #region ADD – Smoke Test

    [Fact]
    public async Task SmokeTest_AddUser_ProducesAudit()
    {
        // Arrange
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = AuditInterceptorTestHelpers.CreateSampleUser("smoketest", "smoke@example.com");

        // Act
        appDb.Users.Add(user);
        await appDb.SaveChangesAsync();

        // Assert
        var audit = await auditDb.AuditLogs.FirstOrDefaultAsync(a => a.EntityId == user.Id);

        Assert.NotNull(audit);
        Assert.Equal("User", audit!.EntityName);
        Assert.Equal("ADDED", audit.Action.ToUpperInvariant());
    }

    #endregion

    #region MODIFY – Smoke Test

    [Fact]
    public async Task SmokeTest_ModifyUser_ProducesAudit()
    {
        // Arrange
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = await AddUserAsync(appDb, "before", "before@example.com");

        // Act
        user.UserName = "after";
        await appDb.SaveChangesAsync();

        // Assert
        var audit = await LatestAuditAsync(auditDb);

        Assert.NotNull(audit);
        Assert.Equal("MODIFIED", audit!.Action.ToUpperInvariant());
        Assert.Contains("UserName", audit.ChangedProperties);
    }

    #endregion

    #region DELETE (Soft Delete) – Smoke Test

    [Fact]
    public async Task SmokeTest_SoftDelete_ProducesAudit()
    {
        // Arrange
        var (appDb, auditDb, _) = AuditInterceptorTestHelpers.CreateDbContextsWithInterceptor();

        var user = await AddUserAsync(appDb, "toDelete", "delete@example.com");

        // Act
        appDb.Users.Remove(user);
        await appDb.SaveChangesAsync();

        // Assert – AuditEntry
        var audit = await LatestAuditAsync(auditDb);

        Assert.NotNull(audit);
        Assert.Equal("DELETED", audit!.Action.ToUpperInvariant());

        // Assert – SoftDelete applied to entity
        var deletedUser = await appDb.Users
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == user.Id);

        Assert.True(deletedUser.IsDeleted);
        Assert.NotNull(deletedUser.DeletedAt);
    }

    #endregion
}
