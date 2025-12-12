using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.TrainingProgram
{
    public class TrainingProgramGetByIdDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsWeekDaySynced { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public List<ProgramDayDto> Days { get; set; } = new();
    }
}