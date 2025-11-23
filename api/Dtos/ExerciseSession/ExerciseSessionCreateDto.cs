using System.ComponentModel.DataAnnotations;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSessionCreateDto
    {
        [Required]
        public int ExerciseId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}