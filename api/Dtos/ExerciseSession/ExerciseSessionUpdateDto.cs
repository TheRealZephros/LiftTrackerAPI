using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.ExerciseSession
{
    public class ExerciseSessionUpdateDto
    {
        [Required]
        public int ExerciseId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}