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
            // Set default schema for audit tables
            modelBuilder.HasDefaultSchema("audit");

            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(a => a.Id);

                b.Property(a => a.EntityName).HasMaxLength(128);
                b.Property(a => a.Action).HasMaxLength(32);
                b.Property(a => a.CorrelationId).HasMaxLength(128);
                b.Property(a => a.IpAddress).HasMaxLength(64);
            });
        }
    }
}
