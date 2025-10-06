using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using api.Dtos.ExerciseSet;
using api.Models;

namespace api.Mappers
{
    public static class ExerciseSetMapper
    {
        public static ExerciseSetDto ToExerciseSetDto(this ExerciseSet set)
        {
            return new ExerciseSetDto
            {
                Id = set.Id,
                Repetitions = set.Repetitions,
                Weight = set.Weight,
            };
        }
    }
}