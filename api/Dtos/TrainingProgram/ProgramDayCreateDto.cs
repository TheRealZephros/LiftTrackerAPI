using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayCreateDto
    {
        public required int TrainingProgramId { get; set; }
        public required int Position { get; set; }
    }
    
}