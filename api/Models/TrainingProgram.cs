using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace api.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public User? User { get; set; } // Navigation property
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ProgramDay> Days { get; set; } = new List<ProgramDay>();
    }
}