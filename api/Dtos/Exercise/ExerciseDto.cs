using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Exercise
{
    public class ExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUsermade { get; set; } = true;
        public int? UserId { get; set; } // Nullable for predefined exercises
    }
}