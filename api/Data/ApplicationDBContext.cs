using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<ProgramDay> ProgramDays { get; set; }
        public DbSet<ProgrammedExercise> ProgrammedExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseSession> ExerciseSessions { get; set; }
        public DbSet<ExerciseSet> ExerciseSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ExerciseSession → Exercise (restrict delete)
            modelBuilder.Entity<ExerciseSession>()
                .HasOne<Exercise>() // you don't need the navigation property
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSet → Exercise (restrict delete)
            modelBuilder.Entity<ExerciseSet>()
                .HasOne<Exercise>() // no nav property required
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSet → ExerciseSession (restrict delete)
            modelBuilder.Entity<ExerciseSet>()
                .HasOne<ExerciseSession>()
                .WithMany(s => s.Sets)
                .HasForeignKey(s => s.ExerciseSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSession → User (restrict delete)
            modelBuilder.Entity<ExerciseSession>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}