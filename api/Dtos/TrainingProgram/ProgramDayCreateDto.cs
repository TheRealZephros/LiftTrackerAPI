using System.ComponentModel.DataAnnotations;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayCreateDto
    {
        [Required]
        public int TrainingProgramId { get; set; }
        public string Name { get; set; } = string.Empty;
        [Required]
        public int Position { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    
}