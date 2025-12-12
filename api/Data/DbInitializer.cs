using System.Text.Json;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                // Check if role already exists
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(UserManager<User> userManager)
        {
            string adminEmail = "admin@example.com";
            string password = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    Id = "11111111-1111-1111-1111-111111111111", // Fixed ID for admin to make seeding exercise sessions easier
                    UserName = adminEmail,
                    Email = adminEmail
                };
                var result = await userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        public static async Task SeedExercisesAsync(ApplicationDbContext context, string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"JSON file not found: {jsonFilePath}");

            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            var exercisesFromJson = JsonSerializer.Deserialize<List<Exercise>>(jsonData);

            if (exercisesFromJson == null || exercisesFromJson.Count == 0)
                return;

            // Step 1: Get all predefined exercise names (UserId == null) from the DB
            var existingPredefinedNames = await context.Exercises
                .Where(e => e.UserId == null)
                .Select(e => e.Name.ToLower())
                .ToListAsync();

            // Step 2: Filter only new exercises that don't already exist as predefined
            var newExercises = exercisesFromJson
                .Where(e => !existingPredefinedNames.Contains(e.Name.ToLower()))
                .Select(e => new Exercise
                {
                    Name = e.Name,
                    Description = e.Description,
                    IsUsermade = false,  // Predefined exercise
                    UserId = null,       // No user
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            // Step 3: Bulk add new exercises
            if (newExercises.Count > 0)
            {
                context.Exercises.AddRange(newExercises);
                await context.SaveChangesAsync();
            }
        }

        // 3 months of beginner sessions
        public static async Task SeedExerciseSessionsAsync(ApplicationDbContext context)
        {
            string userId = "11111111-1111-1111-1111-111111111111"; // Admin user ID
            var random = new Random();

            // Skip if already seeded
            bool alreadySeeded = await context.ExerciseSessions.AnyAsync(es => es.UserId == userId);
            if (alreadySeeded) return;

            var sessions = new List<ExerciseSession>();
            DateTime startDate = DateTime.UtcNow.AddDays(-12 * 7); // 12 weeks ago

            decimal benchWeight = 40m;
            decimal squatWeight = 50m;

            for (int week = 0; week < 12; week++)
            {
                for (int day = 0; day < 3; day++)
                {
                    DateTime sessionDate = startDate.AddDays(week * 7 + day * 2); // M/W/F-ish

                    int exerciseId = (day % 2 == 0) ? 1 : 2;
                    decimal baseWeight = exerciseId == 1 ? benchWeight : squatWeight;
                    int baseReps = exerciseId == 1 ? 8 : 10;

                    var session = new ExerciseSession
                    {
                        UserId = userId,
                        ExerciseId = exerciseId,
                        CreatedAt = sessionDate,
                        Notes = "Good progress"
                    };

                    context.ExerciseSessions.Add(session);
                    await context.SaveChangesAsync(); // Save to get session ID

                    int sets = 3;

                    for (int s = 0; s < sets; s++)
                    {
                        // Randomize weight ±2–5%
                        var weightVariation = baseWeight * (decimal)(random.NextDouble() * 0.05 - 0.025);
                        decimal setWeight = Math.Round(baseWeight + weightVariation, 2);

                        // Randomize reps ±1
                        int repVariation = random.Next(-1, 2);
                        int setReps = Math.Max(1, baseReps + repVariation); // min 1 rep

                        session.Sets.Add(new ExerciseSet
                        {
                            ExerciseSessionId = session.Id,
                            ExerciseId = exerciseId,
                            Repetitions = setReps,
                            Weight = setWeight,
                            CreatedAt = sessionDate
                        });
                    }

                    sessions.Add(session);
                }

                // Weekly progression
                benchWeight += 1.5m;
                squatWeight += 2.5m;
            }

            // Add all sessions and sets in one go
            context.ExerciseSessions.AddRange(sessions);
            await context.SaveChangesAsync();
        }
    }
}
