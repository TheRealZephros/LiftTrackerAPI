using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.ExerciseSession
{
    public class ExerciseSessionDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ExerciseSetDto> Sets { get; set; } = new List<ExerciseSetDto>();
        public string Notes { get; set; } = string.Empty;
    }
}