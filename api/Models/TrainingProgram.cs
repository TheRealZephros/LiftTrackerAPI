using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace api.models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        // Navigation property
        public User? User { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ProgramDay> Days { get; set; } = new List<ProgramDay>();
    }
}