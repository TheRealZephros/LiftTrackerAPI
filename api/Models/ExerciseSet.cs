using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.models
{
    public class ExerciseSet
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        // Navigation property
        public Exercise? Exercise { get; set; }
        public int Repetitions { get; set; }
         // in kg
        private decimal _weight;
        public decimal Weight
        {
            get => Math.Round(_weight, 2);
            set => _weight = Math.Round(value, 2);
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}