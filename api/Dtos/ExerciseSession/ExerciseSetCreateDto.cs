using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.ExerciseSession
{
    public class ExerciseSetCreateDto
    {
        [Required]
        public int ExerciseSessionId { get; set; }
        [Required]
        public int Repetitions { get; set; }
        [Required]
        public decimal Weight { get; set; }
    }
}