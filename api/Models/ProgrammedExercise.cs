using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ProgrammedExercise
    {
        public int Id { get; set; }
        [Required]
        public int ProgramDayId { get; set; } // Foreign key to ProgramDay
        // navigation property
        public ProgramDay? ProgramDay { get; set; }
        [Required]
        public int ExerciseId { get; set; } // Foreign key to Exercise
        // navigation property
        public Exercise? Exercise { get; set; }
        [Required]
        public int Position { get; set; } // Order in the day
        public int Sets { get; set; } = 3;
        public int Reps { get; set; } = 10;
        public double RestTime { get; set; } = 60; // In seconds
        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}