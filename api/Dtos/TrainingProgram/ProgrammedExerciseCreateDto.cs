using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.TrainingProgram
{
    public class ProgrammedExerciseCreateDto
    {
        public required string UserId { get; set; }
        public required int ProgramDayId { get; set; }
        public required int ExerciseId { get; set; }
        public required int Position { get; set; } // Order in the day
        public int Sets { get; set; } = 3;
        public int Reps { get; set; } = 10;
        public double RestTime { get; set; } = 60; // In seconds
        public string Notes { get; set; } = "";
    }
}