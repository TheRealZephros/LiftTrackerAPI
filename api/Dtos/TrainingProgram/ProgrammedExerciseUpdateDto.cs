using System.ComponentModel.DataAnnotations;


namespace Api.Dtos.TrainingProgram
{
    public class ProgrammedExerciseUpdateDto
    {
        [Required]
        public int Position { get; set; } // Order in the day
        public int Sets { get; set; } = 3;
        public int Reps { get; set; } = 10;
        public double RestTime { get; set; } = 60; // In seconds
        public string Notes { get; set; } = "";
    }
}