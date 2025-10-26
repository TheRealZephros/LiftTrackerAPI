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
        public static TrainingProgramDto ToTrainingProgramDto(this TrainingProgram program)
        {
            return new TrainingProgramDto
            {
                Id = program.Id,
                UserId = program.UserId,
                Name = program.Name,
                Description = program.Description,
                IsWeekDaySynced = program.IsWeekDaySynced,
                CreatedAt = program.CreatedAt,
                Days = program.Days?.Select(d => d.ToProgramDayDto()).ToList() ?? new List<ProgramDayDto>()
            };
        }

        public static ProgramDayDto ToProgramDayDto(this ProgramDay day)
        {
            return new ProgramDayDto
            {
                Id = day.Id,
                Name = day.Name,
                TrainingProgramId = day.TrainingProgramId,
                Position = day.Position,
                Description = day.Description,
                Notes = day.Notes,
                Exercises = day.Exercises?.Select(e => e.ToProgrammedExerciseDto()).ToList() ?? new List<ProgrammedExerciseDto>()
            };
        }

        public static ProgrammedExerciseDto ToProgrammedExerciseDto(this ProgrammedExercise exercise)
        {
            return new ProgrammedExerciseDto
            {
                Id = exercise.Id,
                ProgramDayId = exercise.ProgramDayId,
                ExerciseId = exercise.ExerciseId,
                Position = exercise.Position,
                Sets = exercise.Sets,
                Reps = exercise.Reps,
                RestTime = exercise.RestTime,
                Notes = exercise.Notes
            };
        }

        public static TrainingProgramCreateDto ToTrainingProgramCreateDto(this TrainingProgram program)
        {
            return new TrainingProgramCreateDto
            {
                Name = program.Name,
                Description = program.Description,
                IsWeekDaySynced = program.IsWeekDaySynced,
            };
        }


        public static ProgramDay ToProgramDay(this ProgramDayCreateDto dto)
        {
            return new ProgramDay
            {
                Name = dto.Name,
                TrainingProgramId = dto.TrainingProgramId,
                Position = dto.Position,
                Description = dto.Description,
                Notes = dto.Notes,
                Exercises = new List<ProgrammedExercise>()
            };
        }

        public static ProgramDayCreateDto ToProgramDayCreateDto(this ProgramDay day)
        {
            return new ProgramDayCreateDto
            {
                Name = day.Name,
                TrainingProgramId = day.TrainingProgramId,
                Position = day.Position,
                Description = day.Description,
                Notes = day.Notes
            };
        }

        public static ProgrammedExercise ToProgrammedExercise(this ProgrammedExerciseCreateDto dto)
        {
            return new ProgrammedExercise
            {
                ProgramDayId = dto.ProgramDayId,
                ExerciseId = dto.ExerciseId,
                Position = dto.Position,
                Sets = dto.Sets,
                Reps = dto.Reps,
                RestTime = dto.RestTime,
                Notes = dto.Notes
            };
        }

        public static ProgrammedExerciseCreateDto ToProgrammedExerciseCreateDto(this ProgrammedExercise exercise)
        {
            return new ProgrammedExerciseCreateDto
            {
                ProgramDayId = exercise.ProgramDayId,
                ExerciseId = exercise.ExerciseId,
                Position = exercise.Position,
                Sets = exercise.Sets,
                Reps = exercise.Reps,
                RestTime = exercise.RestTime,
                Notes = exercise.Notes
            };
        }

        public static ProgramDayDto ToProgramDayDto(this ProgramDayCreateDto dto)
        {
            return new ProgramDayDto
            {
                Name = dto.Name,
                TrainingProgramId = dto.TrainingProgramId,
                Position = dto.Position,
                Description = dto.Description,
                Notes = dto.Notes,
                Exercises = new List<ProgrammedExerciseDto>()
            };
        }
    }
}