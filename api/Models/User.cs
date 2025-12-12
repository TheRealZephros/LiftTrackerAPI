using Api.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Api.Models
{
    public class User : IdentityUser, ISoftDeletable
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();
        public List<ExerciseSession> ExerciseSessions { get; set; } = new();
        public List<Exercise> Exercises { get; set; } = new();
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}