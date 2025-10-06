using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public List<ProgramDay> Days { get; set; } = [];
    }
}