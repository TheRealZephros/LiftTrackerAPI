using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ProgrammedExercise
    {
        public int Id { get; set; }
        public required string UserId { get; set; } // Foreign key to User
        public required int ProgramDayId { get; set; } // Foreign key to ProgramDay
        public required int ExerciseId { get; set; } // Foreign key to Exercise
        public required int Position { get; set; } // Order in the day
        public int Sets { get; set; } = 3;
        public int Reps { get; set; } = 10;
        public double RestTime { get; set; } = 60; // In seconds
        public string Notes { get; set; } = "";
    }
}