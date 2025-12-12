using api.Data;
using api.Interfaces;
using api.Models;
using api.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace api.Infrastructure.Auditing
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICorrelationIdAccessor _correlation;
        private readonly IUserContext _user;
        private readonly IHttpContextInfo _http;
        private readonly AuditDbContext _audit;

        public AuditSaveChangesInterceptor(
            ICorrelationIdAccessor corr,
            IUserContext user,
            IHttpContextInfo http,
            AuditDbContext auditDb)
        {
            _correlation = corr;
            _user = user;
            _http = http;
            _audit = auditDb;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            var entries = context.ChangeTracker.Entries()
                .Where(e =>
                    e.Entity is not AuditLog &&
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var e in entries)
            {
                HandleEntry(e);
            }

            await _audit.SaveChangesAsync(ct);

            return result;
        }

        private void HandleEntry(EntityEntry e)
        {
            var entityName = e.Entity.GetType().Name;
            var entityId = e.Property("Id").CurrentValue?.ToString();

            string action = e.State switch
            {
                EntityState.Added => "ADDED",
                EntityState.Modified => "MODIFIED",
                EntityState.Deleted => "DELETED",
                _ => "UNKNOWN"
            };

            var changedProps = new List<string>();
            var oldVals = new Dictionary<string, object?>();
            var newVals = new Dictionary<string, object?>();

            if (e.State == EntityState.Modified)
            {
                foreach (var prop in e.Properties)
                {
                    if (!prop.IsModified) continue;

                    changedProps.Add(prop.Metadata.Name);
                    oldVals[prop.Metadata.Name] = prop.OriginalValue;
                    newVals[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            if (e.State == EntityState.Deleted && e.Entity is ISoftDeletable del)
            {
                // Soft delete
                del.IsDeleted = true;
                del.DeletedAt = DateTime.UtcNow;
                e.State = EntityState.Modified;

                changedProps.Add(nameof(ISoftDeletable.IsDeleted));
                changedProps.Add(nameof(ISoftDeletable.DeletedAt));
            }

            _audit.AuditLogs.Add(new AuditLog
            {
                EntityId = entityId ?? "",
                EntityName = entityName,
                Action = action,
                ChangedProperties = JsonSerializer.Serialize(changedProps),
                OldValues = oldVals.Any() ? JsonSerializer.Serialize(oldVals) : null,
                NewValues = newVals.Any() ? JsonSerializer.Serialize(newVals) : null,
                PerformedByUserId = _user.UserId,
                PerformedByUserName = _user.UserName,
                PerformedByUserEmail = _user.Email,
                CorrelationId = _correlation.CorrelationId,
                IpAddress = _http.IpAddress,
                UserAgent = _http.UserAgent,
            });
        }
    }
}
