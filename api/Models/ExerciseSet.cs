using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ExerciseSet
    {
        public int Id { get; set; }
        public required int ExerciseId { get; set; }
        public required int ExerciseSessionId { get; set; }
        public required int Repetitions { get; set; }
         // in kg
        private decimal _weight;
        public required decimal Weight
        {
            get => Math.Round(_weight, 2);
            set => _weight = Math.Round(value, 2);
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}