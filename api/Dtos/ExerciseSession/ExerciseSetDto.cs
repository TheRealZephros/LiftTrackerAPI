using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSetDto
    {
        public int Id { get; set; }
        public required int ExerciseId { get; set; }
        public required int ExerciseSessionId { get; set; }
        public required int Repetitions { get; set; }
        public required decimal Weight { get; set; } // in kg
    }
}