using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Exercise;
using api.Models;

namespace api.Interfaces
{
    public interface IExerciseRepository
    {
        Task<bool> ExerciseExists(string userId, int exerciseId);
        Task<Exercise?> GetByIdAsync(string userId, int id);
        Task<List<Exercise>> GetAllAsync(string userId);
        Task<Exercise> AddAsync(string userId, ExerciseCreateDto exerciseDto);
        Task<Exercise?> UpdateAsync(int id, ExerciseUpdateDto exerciseUpdateDto);
        Task<Exercise?> DeleteAsync(int id);

    }
}