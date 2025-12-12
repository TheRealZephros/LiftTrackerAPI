using Api.Models.Interfaces;

namespace Api.Models
{
    public class ExerciseSet : ISoftDeletable
    {
        public int Id { get; set; }
        public required int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; } // Navigation property
        public required int ExerciseSessionId { get; set; }
        public ExerciseSession? ExerciseSession { get; set; } // Navigation property
        public required int Repetitions { get; set; }
         // in kg
        private decimal _weight;
        public required decimal Weight
        {
            get => Math.Round(_weight, 2);
            set => _weight = Math.Round(value, 2);
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}