using System.Text.Json;
using api.Data;
using api.Interfaces;
using api.Models;
using api.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace api.Infrastructure.Auditing
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        #region FIELDS

        private readonly ICorrelationIdAccessor _correlation;
        private readonly IUserContext _user;
        private readonly IHttpContextInfo _http;
        private readonly AuditDbContext _auditDb;

        #endregion

        #region CONSTRUCTOR

        public AuditSaveChangesInterceptor(
            ICorrelationIdAccessor correlation,
            IUserContext user,
            IHttpContextInfo http,
            AuditDbContext auditDb)
        {
            _correlation = correlation;
            _user = user;
            _http = http;
            _auditDb = auditDb;
        }

        #endregion

        #region OVERRIDES - SavingChanges (SYNC & ASYNC)

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ProcessChanges(eventData.Context);
            _auditDb.SaveChanges(); // sync write
            return result;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            ProcessChanges(eventData.Context);
            await _auditDb.SaveChangesAsync(ct); // async write
            return result;
        }

        #endregion

        #region CHANGE PROCESSING

        /// <summary>
        /// Identifies added, modified, and deleted entities and generates audit records.
        /// </summary>
        private void ProcessChanges(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e =>
                    e.Entity is not AuditLog &&
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var e in entries)
            {
                HandleEntry(e);
            }
        }

        #endregion

        #region ENTRY HANDLING

        private void HandleEntry(EntityEntry entry)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Property("Id").CurrentValue?.ToString() ?? "";

            // Determine action
            var action = entry.State switch
            {
                EntityState.Added => "ADDED",
                EntityState.Modified => "MODIFIED",
                EntityState.Deleted => "DELETED",
                _ => "UNKNOWN"
            };

            var changedProperties = new List<string>();
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            #region MODIFIED ENTRIES

            if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (!prop.IsModified) continue;

                    changedProperties.Add(prop.Metadata.Name);
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            #endregion

            #region SOFT DELETE HANDLING

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable soft)
            {
                // Perform soft delete instead of physical delete
                soft.IsDeleted = true;
                soft.DeletedAt = DateTime.UtcNow;

                entry.State = EntityState.Modified;

                changedProperties.Add(nameof(ISoftDeletable.IsDeleted));
                changedProperties.Add(nameof(ISoftDeletable.DeletedAt));
            }

            #endregion

            #region WRITE AUDIT LOG

            _auditDb.AuditLogs.Add(new AuditLog
            {
                EntityId = entityId,
                EntityName = entityName,
                Action = action,

                ChangedProperties = JsonSerializer.Serialize(changedProperties),
                OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null,

                PerformedByUserId = _user.UserId,
                PerformedByUserName = _user.UserName,
                PerformedByUserEmail = _user.Email,

                CorrelationId = _correlation.CorrelationId,
                IpAddress = _http.IpAddress,
                UserAgent = _http.UserAgent
            });

            #endregion
        }

        #endregion
    }
}
