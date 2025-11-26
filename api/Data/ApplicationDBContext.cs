using System.Linq.Expressions;
using api.Models;
using api.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<ProgramDay> ProgramDays { get; set; }
        public DbSet<ProgrammedExercise> ProgrammedExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseSession> ExerciseSessions { get; set; }
        public DbSet<ExerciseSet> ExerciseSets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(
                        BuildIsDeletedRestriction(entityType.ClrType)
                    );
            }
    }

            // ----------------------------
            // Configure decimal precision
            // ----------------------------
            modelBuilder.Entity<ExerciseSet>()
                .Property(es => es.Weight)
                .HasPrecision(6, 2); // max 9999.99

            // ----------------------------
            // USER → TRAINING PROGRAM (Cascade)
            // ----------------------------
            modelBuilder.Entity<User>()
                .HasMany(u => u.TrainingPrograms)
                .WithOne(tp => tp.User)
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TRAINING PROGRAM → PROGRAM DAYS (Cascade)
            modelBuilder.Entity<TrainingProgram>()
                .HasMany(tp => tp.Days)
                .WithOne(d => d.TrainingProgram)
                .HasForeignKey(d => d.TrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // PROGRAM DAY → PROGRAMMED EXERCISES (Cascade)
            modelBuilder.Entity<ProgramDay>()
                .HasMany(pd => pd.Exercises)
                .WithOne(pe => pe.ProgramDay)
                .HasForeignKey(pe => pe.ProgramDayId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------
            // USER → EXERCISES (Cascade)
            // ----------------------------
            modelBuilder.Entity<User>()
                .HasMany(u => u.Exercises)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // USER → EXERCISE SESSIONS (Cascade)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ExerciseSessions)
                .WithOne(es => es.User)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // EXERCISE SESSION → EXERCISE SETS (Cascade)
            modelBuilder.Entity<ExerciseSession>()
                .HasMany(es => es.Sets)
                .WithOne(s => s.ExerciseSession)
                .HasForeignKey(s => s.ExerciseSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------
            // EXERCISE RELATIONSHIPS (Restrict)
            // ----------------------------
            // ProgrammedExercise → Exercise (restrict)
            modelBuilder.Entity<ProgrammedExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany()
                .HasForeignKey(pe => pe.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSession → Exercise (restrict)
            modelBuilder.Entity<ExerciseSession>()
                .HasOne(es => es.Exercise)
                .WithMany()
                .HasForeignKey(es => es.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSet → Exercise (restrict)
            modelBuilder.Entity<ExerciseSet>()
                .HasOne(es => es.Exercise)
                .WithMany()
                .HasForeignKey(es => es.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        private static LambdaExpression BuildIsDeletedRestriction(Type entityType)
        {
            var param = Expression.Parameter(entityType, "e");
            var prop = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
            var condition = Expression.Equal(prop, Expression.Constant(false));
            return Expression.Lambda(condition, param);
        }
    }
}