using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSetUpdateDto
    {
        public required int Repetitions { get; set; }
        public required decimal Weight { get; set; } // in kg
    }
}