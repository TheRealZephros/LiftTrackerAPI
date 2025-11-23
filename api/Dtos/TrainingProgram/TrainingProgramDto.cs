using System.ComponentModel.DataAnnotations;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramDto
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public List<ProgramDayDto> Days { get; set; } = new();
    }
}