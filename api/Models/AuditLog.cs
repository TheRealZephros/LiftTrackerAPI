using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // Changes made to which entity
        [Required]
        public string EntityName { get; set; } = string.Empty;// e.g. "User", "Exercise", "TrainingProgram"
        // e.g. UserId, ExerciseId
        [Required]
        public string EntityId { get; set; } = string.Empty; 
        public string Action { get; set; } = string.Empty; // CREATE / UPDATE / DELETE
        
        // Diff only for UPDATE actions
        [Required]
        public string ChangedProperties { get; set; } = string.Empty; // Comma-separated list of changed properties
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        // User performing the action
        public string? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }
        public string? PerformedByUserEmail { get; set; }

        // When the action was performed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Where the action originated from
        public string? CorrelationId { get; set; } // Correlation id for log tracing
        public string? IpAddress { get; set; } // IP address of the user
        public string? UserAgent { get; set; } // User agent string

        public string? Source { get; set; } // Source of the change (e.g., "WebApp", "MobileApp", "API")

    }
}