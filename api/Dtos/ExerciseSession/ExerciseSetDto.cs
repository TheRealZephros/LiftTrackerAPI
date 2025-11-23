using System.ComponentModel.DataAnnotations;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSetDto
    {
        public int Id { get; set; }
        [Required]
        public int ExerciseId { get; set; }
        [Required]
        public int ExerciseSessionId { get; set; }
        [Required]
        public int Repetitions { get; set; }
        [Required]
        public decimal Weight { get; set; } // in kg
    }
}