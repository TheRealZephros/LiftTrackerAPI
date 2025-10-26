using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class TrainingProgramGetAllDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
    }
}