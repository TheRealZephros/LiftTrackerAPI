using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramCreateDto
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsWeekDaySynced { get; set; } = true;
    }
}