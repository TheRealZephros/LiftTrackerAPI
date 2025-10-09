using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.ExerciseSession;
using api.Models;

namespace api.Interfaces
{
    public interface IExerciseSessionRepository
    {

        Task<bool> ExerciseSessionExists(int sessionId);
        Task<bool> ExerciseSetExists(int setId);
        Task<ExerciseSession?> GetByIdAsync(int id);
        Task<List<ExerciseSet>> GetSetsBySessionIdAsync(int sessionId);
        Task<ExerciseSet?> GetSetByIdAsync(int setId);
        Task<List<ExerciseSession>> GetSessionsByExerciseId(int exerciseId);
        Task<List<ExerciseSet>?> GetSetsByExerciseId(int exerciseId);
        Task<List<ExerciseSession>> GetAllAsync(int userId);
        Task<ExerciseSession?> AddAsync(ExerciseSessionCreateDto exerciseSessionDto);
        Task<ExerciseSet?> AddSetAsync(ExerciseSetCreateDto exerciseSetDto);
        Task<ExerciseSession?> UpdateAsync(int id, ExerciseSessionDto exerciseSessionDto);
        Task<ExerciseSet?> UpdateSetAsync(int id, ExerciseSetDto exerciseSetDto);
        Task<ExerciseSession?> DeleteAsync(int id);
        Task<ExerciseSet?> DeleteSetAsync(int id);
    }
}