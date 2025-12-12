using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos.Exercise;
using Api.Models;

namespace Api.Interfaces
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