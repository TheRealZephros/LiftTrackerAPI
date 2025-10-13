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
        Task<bool> ExerciseExists(int exerciseId);
        Task<Exercise?> GetByIdAsync(int id);
        Task<List<Exercise>> GetAllAsync(string userId);
        Task<Exercise?> AddAsync(ExerciseDto exerciseDto);
        Task<Exercise?> UpdateAsync(int id, ExerciseDto exerciseDto);
        Task<Exercise?> DeleteAsync(int id);

    }
}