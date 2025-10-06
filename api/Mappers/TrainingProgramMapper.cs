using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using api.Dtos.TrainingProgram;
using api.Models;

namespace api.Mappers
{
    public static class TrainingProgramMapper
    {
        public static TrainingProgramCreateDto ToTrainingProgramCreateDto(this TrainingProgram program)
        {
            return new TrainingProgramCreateDto
            {
                UserId = program.UserId,
                Name = program.Name,
                Description = program.Description,
                IsWeekDaySynced = program.IsWeekDaySynced,
            };
        }

        public static TrainingProgram ToTrainingProgram(this TrainingProgramCreateDto dto)
        {
            return new TrainingProgram
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Description = dto.Description,
                IsWeekDaySynced = dto.IsWeekDaySynced,
                Days = new List<ProgramDay>()
            };
        }

        public static ProgramDay ToProgramDay(this ProgramDayCreateDto dto)
        {
            return new ProgramDay
            {
                TrainingProgramId = dto.TrainingProgramId,
                Position = dto.Position,
                Exercises = new List<Exercise>()
            };
        }

        public static ProgramDayCreateDto ToProgramDayCreateDto(this ProgramDay day)
        {
            return new ProgramDayCreateDto
            {
                TrainingProgramId = day.TrainingProgramId,
                Position = day.Position,
            };
        }
    }
}