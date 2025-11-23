using System.ComponentModel.DataAnnotations;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayUpdateDto
    {
        [Required]
        public int Position { get; set; }
        public string Description { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Name { get; set; } = "";
    }
}