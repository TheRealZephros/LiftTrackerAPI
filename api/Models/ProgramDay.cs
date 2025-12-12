using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Interfaces;
using Microsoft.Net.Http.Headers;

namespace Api.Models
{
    public enum Weekday
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }

    public class ProgramDay : ISoftDeletable
    {
        public int Id { get; set; }
        public required int TrainingProgramId { get; set; }
        // Navigation property
        public TrainingProgram? TrainingProgram { get; set; }
        
        public int Position { get; set; } // Order in the week or program
        public bool IsWeekDaySynced { get; set; } = true;
        public List<ProgrammedExercise> Exercises { get; set; } = new();
        public string Description { get; set; } = "";
        public string Notes { get; set; } = "";
        private string _name = "";
        public string Name
        {
            get
            {
                if (_name == "")
                {
                    if (IsWeekDaySynced)
                        return ((Weekday)Position - 1).ToString();
                    return "Day " + Position.ToString();
                }
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}