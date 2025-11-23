using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public User? User { get; set; } // Navigation property
        [Required]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ProgramDay> Days { get; set; } = new List<ProgramDay>();
    }
}