using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace api.Models
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

    public class ProgramDay
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
    }
}