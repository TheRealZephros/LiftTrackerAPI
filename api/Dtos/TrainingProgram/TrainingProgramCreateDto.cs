using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.TrainingProgram
{
    public class TrainingProgramCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
    }
}