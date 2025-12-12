using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.TrainingProgram
{
    public class TrainingProgramUpdateDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
    }
}