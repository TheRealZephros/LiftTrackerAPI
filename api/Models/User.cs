using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();
        public List<ExerciseSession> ExerciseSessions { get; set; } = new();
        public List<Exercise> Exercises { get; set; } = new();
    }
}