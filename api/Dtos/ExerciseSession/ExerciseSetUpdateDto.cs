using System.ComponentModel.DataAnnotations;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSetUpdateDto
    {
        [Required]
        public int Repetitions { get; set; }
        [Required]
        public decimal Weight { get; set; } // in kg
    }
}