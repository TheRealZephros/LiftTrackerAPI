using Api.Models.Interfaces;

namespace Api.Models
{
    public class Exercise : ISoftDeletable
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUsermade { get; set; } = true;
        public string? UserId { get; set; } // Nullable for predefined exercises
        public User? User { get; set; } // Navigation property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}