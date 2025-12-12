using System.Linq.Expressions;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ----------------------------
        // DBSets
        // ----------------------------
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<ProgramDay> ProgramDays { get; set; }
        public DbSet<ProgrammedExercise> ProgrammedExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseSession> ExerciseSessions { get; set; }
        public DbSet<ExerciseSet> ExerciseSets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // ----------------------------
        // MODEL CONFIGURATION
        // ----------------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureSoftDeleteFilters(modelBuilder);
            ConfigurePrecision(modelBuilder);
            ConfigureUserRelationships(modelBuilder);
            ConfigureTrainingProgramRelationships(modelBuilder);
            ConfigureExerciseHierarchy(modelBuilder);
            ConfigureRestrictRules(modelBuilder);
        }

        // ----------------------------
        // Soft Delete Global Filters
        // ----------------------------
        private void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var filter = BuildSoftDeleteFilter(entityType.ClrType);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
        {
            var param = Expression.Parameter(entityType, "e");
            var isDeletedProp = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
            var predicate = Expression.Equal(isDeletedProp, Expression.Constant(false));
            return Expression.Lambda(predicate, param);
        }

        // ----------------------------
        // Decimal Precision
        // ----------------------------
        private static void ConfigurePrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExerciseSet>()
                .Property(es => es.Weight)
                .HasPrecision(6, 2);
        }

        // ----------------------------
        // User → Programs, Exercises, Sessions
        // ----------------------------
        private static void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            // User → TrainingPrograms (Cascade)
            modelBuilder.Entity<User>()
                .HasMany(u => u.TrainingPrograms)
                .WithOne(tp => tp.User)
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Exercises (Cascade)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Exercises)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → ExerciseSessions (Cascade)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ExerciseSessions)
                .WithOne(es => es.User)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // ----------------------------
        // TrainingProgram → Days → ProgrammedExercises
        // ----------------------------
        private static void ConfigureTrainingProgramRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainingProgram>()
                .HasMany(tp => tp.Days)
                .WithOne(d => d.TrainingProgram)
                .HasForeignKey(d => d.TrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProgramDay>()
                .HasMany(d => d.Exercises)
                .WithOne(pe => pe.ProgramDay)
                .HasForeignKey(pe => pe.ProgramDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // ----------------------------
        // ExerciseSession → Sets
        // ----------------------------
        private static void ConfigureExerciseHierarchy(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExerciseSession>()
                .HasMany(es => es.Sets)
                .WithOne(s => s.ExerciseSession)
                .HasForeignKey(s => s.ExerciseSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // ----------------------------
        // Restrict Delete rules for exercises
        // ----------------------------
        private static void ConfigureRestrictRules(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProgrammedExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany()
                .HasForeignKey(pe => pe.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExerciseSession>()
                .HasOne(es => es.Exercise)
                .WithMany()
                .HasForeignKey(es => es.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExerciseSet>()
                .HasOne(es => es.Exercise)
                .WithMany()
                .HasForeignKey(es => es.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
