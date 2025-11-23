using System.Text.Json;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Data
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
                var admin = new User { UserName = adminEmail, Email = adminEmail };
                var result = await userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        public static async Task SeedExercisesAsync(ApplicationDBContext context, string jsonFilePath)
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
        
    }
}
