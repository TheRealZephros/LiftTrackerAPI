using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramGetByIdDto
    {
        public required int Id { get; set; }
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsWeekDaySynced { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required List<ProgramDayDto> Days { get; set; } = new();
    }
}