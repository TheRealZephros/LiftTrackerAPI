using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options)
            : base(options) { }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DO NOT set schema in-memory — EF Core will drop all writes silently
            if (Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                modelBuilder.HasDefaultSchema("audit");
            }

            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(a => a.Id);
                b.Property(a => a.EntityName).HasMaxLength(128);
                b.Property(a => a.Action).HasMaxLength(32);
            });
        }
    }
}
