using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.models
{
    public enum Weekday
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }
    public class ProgramDay
    {

        public int Id { get; set; }
        public int? TrainingProgramId { get; set; }
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