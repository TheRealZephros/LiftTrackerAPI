using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Exercise;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDBContext _context;

        public ExerciseRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Exercise?> GetByIdAsync(string userId, int id)
        {
            return await _context.Exercises.FirstOrDefaultAsync(e => (e.Id == id && e.UserId == userId) || (e.Id == id && !e.IsUsermade));
        }

        public async Task<List<Exercise>> GetAllAsync(string userId)
        {
            return await _context.Exercises.Where(e => e.UserId == userId || !e.IsUsermade).ToListAsync();
        }

        public async Task<Exercise> AddAsync(string userId, ExerciseCreateDto exerciseCreateDto)
        {
            var exercise = new Exercise
            {
                Name = exerciseCreateDto.Name,
                Description = exerciseCreateDto.Description,
                IsUsermade = true,
                UserId = userId
            };
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<Exercise?> UpdateAsync(int id, ExerciseUpdateDto exerciseUpdateDto)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return null;

            exercise.Name = exerciseUpdateDto.Name;
            exercise.Description = exerciseUpdateDto.Description;

            _context.Exercises.Update(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<Exercise?> DeleteAsync(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return null;

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<bool> ExerciseExists(string userId, int exerciseId)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == exerciseId && e.UserId == userId);
        }
    }
}