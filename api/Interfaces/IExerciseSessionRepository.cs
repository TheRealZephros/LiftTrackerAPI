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

        Task<bool> ExerciseSessionExists(string userId, int sessionId);
        Task<bool> ExerciseSetExists(string userId, int setId);
        Task<ExerciseSession?> GetByIdAsync(int id);
        Task<List<ExerciseSet>> GetSetsBySessionIdAsync(int sessionId);
        Task<ExerciseSet?> GetSetByIdAsync(int setId);
        Task<List<ExerciseSession>> GetSessionsByExerciseId(int exerciseId);
        Task<List<ExerciseSet>?> GetSetsByExerciseId(int exerciseId);
        Task<List<ExerciseSession>> GetAllAsync(string userId);
        Task<ExerciseSession?> AddAsync(string userId, ExerciseSessionCreateDto exerciseSessionDto);
        Task<ExerciseSet?> AddSetAsync(int exerciseId, ExerciseSetCreateDto exerciseSetDto);
        Task<ExerciseSession?> UpdateAsync(int id, ExerciseSessionUpdateDto exerciseSessionDto);
        Task<ExerciseSet?> UpdateSetAsync(int id, ExerciseSetUpdateDto exerciseSetDto);
        Task<ExerciseSession?> DeleteAsync(int id);
        Task<ExerciseSet?> DeleteSetAsync(int id);
    }
}