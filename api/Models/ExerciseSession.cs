using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ExerciseSession
    {
        public int Id { get; set; }
        // Foreign key
        public required int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; } // Navigation property
        // Foreign key
        public required string UserId { get; set; }
        public User? User { get; set; } // Navigation property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ExerciseSet> Sets { get; set; } = new List<ExerciseSet>();
        public string Notes { get; set; } = string.Empty;
    }
}