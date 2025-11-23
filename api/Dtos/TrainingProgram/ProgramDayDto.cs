using System.ComponentModel.DataAnnotations;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayDto
    {
        public int Id { get; set; }
        
        [Required]
        public int TrainingProgramId { get; set; }
        [Required]
        public int Position { get; set; } // Order in the week or program
        public string Description { get; set; } = "";
        public string Notes { get; set; } = "";
        private string _name = "";
        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? $"Day {Position}" : _name;
            set => _name = value;
        }
        public List<ProgrammedExerciseDto> Exercises { get; set; } = new();
    }
}