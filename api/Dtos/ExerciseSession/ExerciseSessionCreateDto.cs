using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSessionCreateDto
    {
        public required int ExerciseId { get; set; }
        public required string UserId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}