using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramGetByIdDto
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
    }
}