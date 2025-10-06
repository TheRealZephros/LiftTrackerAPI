using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Exercise;
using api.Dtos.ExerciseSet;
using api.Dtos.ExerciseSession;
using api.Models;

namespace api.Mappers
{
    public static class ExerciseMapper
    {
        public static ExerciseDto ToExerciseDto(this Exercise exercise)
        {
            return new ExerciseDto
            {
                Name = exercise.Name,
                Description = exercise.Description,
                IsUsermade = true,
                UserId = exercise.UserId
            };
        }

        public static Exercise ToExercise(this ExerciseDto dto)
        {
            return new Exercise
            {
                Name = dto.Name,
                Description = dto.Description,
                IsUsermade = dto.IsUsermade,
                UserId = dto.UserId
            };
        }

        public static DeleteExerciseDto ToDeleteExerciseDto(this Exercise exercise)
        {
            return new DeleteExerciseDto
            {
                Id = exercise.Id
            };
        }
        
    }
}