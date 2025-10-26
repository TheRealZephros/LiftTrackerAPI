using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayCreateDto
    {
        public required int TrainingProgramId { get; set; }
        public string Name { get; set; } = string.Empty;
        public required int Position { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    
}