using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public List<Exercise> Exercises { get; set; } = [];
        public string Name
        {
            get {
                if (TrainingProgram != null && TrainingProgram.IsWeekDaySynced)
                {
                    return ((Weekday)Position).ToString();
                }
                return "Day " + Position.ToString();
            }
        }

    }
}