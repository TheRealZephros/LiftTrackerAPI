using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUsermade { get; set; } = true;
        public string? UserId { get; set; } // Nullable for predefined exercises
        public User? User { get; set; } // Navigation property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}