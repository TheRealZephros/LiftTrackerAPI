using api.Data;
using api.Infrastructure.Correlation;
using api.Infrastructure.Security;
using api.Models;
using api.Models.Interfaces;
using api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace api.Infrastructure.Auditing
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<string> IgnoredProperties = new()
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "RefreshToken",
            "AccessToken"
        };

        private readonly ICorrelationIdAccessor _correlation;
        private readonly IUserContext _userContext;
        private readonly IHttpContextInfo _httpContext;
        private readonly AuditDbContext _auditContext;

        public AuditSaveChangesInterceptor(
            ICorrelationIdAccessor correlation,
            IUserContext userContext,
            IHttpContextInfo httpContext,
            AuditDbContext auditContext)
        {
            _correlation = correlation;
            _userContext = userContext;
            _httpContext = httpContext;
            _auditContext = auditContext;
        }

        // Soft deletes BEFORE save
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            HandleSoftDeletes(eventData.Context);
            return result;
        }

        // Audit AFTER save (non-transactional)
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            var auditLogs = CreateAuditEntries(context.ChangeTracker);

            if (auditLogs.Count > 0)
            {
                _auditContext.AuditLogs.AddRange(auditLogs);
                await _auditContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private void HandleSoftDeletes(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        private List<AuditLog> CreateAuditEntries(ChangeTracker tracker)
        {
            var audits = new List<AuditLog>();

            foreach (var entry in tracker.Entries())
            {
                if (entry.Entity is AuditLog ||
                    entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Detached)
                    continue;

                var audit = new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = GetPrimaryKey(entry),
                    Action = entry.State.ToString().ToUpper(),

                    // WHO
                    PerformedByUserId = _userContext.UserId,
                    PerformedByUserName = _userContext.UserName,
                    PerformedByUserEmail = _userContext.Email,

                    // WHEN
                    CreatedAt = DateTime.UtcNow,

                    // WHERE
                    CorrelationId = _correlation.CorrelationId,
                    IpAddress = _httpContext.IpAddress,
                    UserAgent = _httpContext.UserAgent,

                    Source = "API"
                };

                if (entry.State == EntityState.Modified)
                {
                    var diff = GetPropertyDiff(entry);

                    // Skip noise-only updates
                    if (diff.changedProps == "[]")
                        continue;

                    audit.OldValues = diff.oldValues;
                    audit.NewValues = diff.newValues;
                    audit.ChangedProperties = diff.changedProps;
                }
                else if (entry.State == EntityState.Added)
                {
                    audit.NewValues = Serialize(entry.CurrentValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    audit.OldValues = Serialize(entry.OriginalValues);
                }

                audits.Add(audit);
            }

            return audits;
        }

        private static (string oldValues, string newValues, string changedProps) GetPropertyDiff(EntityEntry entry)
        {
            var oldDict = new Dictionary<string, object?>();
            var newDict = new Dictionary<string, object?>();
            var changed = new List<string>();

            foreach (var prop in entry.Properties)
            {
                if (!prop.IsModified) continue;
                if (IgnoredProperties.Contains(prop.Metadata.Name)) continue;

                oldDict[prop.Metadata.Name] = prop.OriginalValue;
                newDict[prop.Metadata.Name] = prop.CurrentValue;
                changed.Add(prop.Metadata.Name);
            }

            return (
                JsonSerializer.Serialize(oldDict),
                JsonSerializer.Serialize(newDict),
                JsonSerializer.Serialize(changed)
            );
        }

        private static string Serialize(PropertyValues values)
        {
            var dict = new Dictionary<string, object?>();

            foreach (var prop in values.Properties)
            {
                if (IgnoredProperties.Contains(prop.Name))
                    continue;

                dict[prop.Name] = values[prop.Name];
            }

            return JsonSerializer.Serialize(dict);
        }

        private static string GetPrimaryKey(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key == null) return string.Empty;

            var values = key.Properties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString());

            return string.Join(",", values);
        }
    }
}
