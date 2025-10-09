using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using api.Dtos.ExerciseSession;
using api.Models;

namespace api.Mappers
{
    public static class ExerciseSessionMapper
    {
        public static ExerciseSessionDto ToExerciseSessionDto(this ExerciseSession session)
        {
            return new ExerciseSessionDto
            {
                Id = session.Id,
                ExerciseId = session.ExerciseId,
                UserId = session.UserId,
                CreatedAt = session.CreatedAt,
                Sets = session.Sets.Select(s => s.ToExerciseSetDto()).ToList(),
                Notes = session.Notes ?? string.Empty
            };
        }
        public static ExerciseSetDto ToExerciseSetDto(this ExerciseSet set)
        {
            return new ExerciseSetDto
            {
                Id = set.Id,
                ExerciseId = set.ExerciseId,
                ExerciseSessionId = set.ExerciseSessionId,
                Repetitions = set.Repetitions,
                Weight = set.Weight,
            };
        }
    }
    
}