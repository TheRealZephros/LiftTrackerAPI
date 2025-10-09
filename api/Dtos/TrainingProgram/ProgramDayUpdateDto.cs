using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class ProgramDayUpdateDto
    {
        public required int Id { get; set; }
        public required int TrainingProgramId { get; set; }
        public int Position { get; set; }
        public string Description { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Name { get; set; } = "";
    }
}