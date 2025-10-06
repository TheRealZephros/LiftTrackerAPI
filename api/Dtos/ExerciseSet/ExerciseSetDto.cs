using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.ExerciseSet
{
    public class ExerciseSetDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int ExerciseSessionId { get; set; }
        public int Repetitions { get; set; }
        public decimal Weight { get; set; } // in kg
    }
}