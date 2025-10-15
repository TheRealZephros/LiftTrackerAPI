using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : IdentityDbContext<User>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<ProgramDay> ProgramDays { get; set; }
        public DbSet<ProgrammedExercise> ProgrammedExercises { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseSession> ExerciseSessions { get; set; }
        public DbSet<ExerciseSet> ExerciseSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            };

            modelBuilder.Entity<IdentityRole>().HasData(roles);

            // ------ Define relationships and cascade delete behaviors ------ //

            // Cascade delete for deleted User -> TrainingProgram -> ProgramDays -> ProgrammedExercises
            modelBuilder.Entity<User>( u => u.HasMany(u => u.TrainingPrograms).WithOne().HasForeignKey(tp => tp.UserId).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<TrainingProgram>( t => t.HasMany(tp => tp.Days).WithOne(d => d.TrainingProgram).HasForeignKey(d => d.TrainingProgramId).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<ProgramDay>(pd => pd.HasMany(d => d.Exercises).WithOne(e => e.ProgramDay).HasForeignKey(e => e.ProgramDayId).OnDelete(DeleteBehavior.Cascade));

            // Cascade delete for deleted User -> ExerciseSessions -> ExerciseSets
            modelBuilder.Entity<User>( u => u.HasMany<ExerciseSession>().WithOne().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade));
            modelBuilder.Entity<ExerciseSession>(s => s.HasMany(s => s.Sets).WithOne().HasForeignKey(s => s.ExerciseSessionId).OnDelete(DeleteBehavior.Cascade));

            // Cascade delete for deleted User -> Exercises
            modelBuilder.Entity<User>(u => u.HasMany<Exercise>().WithOne().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade));
            
            // Cascade delete for deleted ExerciseSession -> ExerciseSets
            modelBuilder.Entity<ExerciseSet>()
                .HasOne<ExerciseSession>()
                .WithMany(s => s.Sets)
                .HasForeignKey(s => s.ExerciseSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            

            // ProgrammedExercise → Exercise (restrict delete)
            modelBuilder.Entity<ProgrammedExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany()
                .HasForeignKey(pe => pe.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSession → Exercise (restrict delete)
            modelBuilder.Entity<ExerciseSession>()
                .HasOne<Exercise>() // you don't need the navigation property
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExerciseSet → Exercise (restrict delete)
            modelBuilder.Entity<ExerciseSet>()
                .HasOne(es => es.Exercise)
                .WithMany()
                .HasForeignKey(es => es.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}