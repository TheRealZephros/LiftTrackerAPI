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

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Exercise>> GetAllAsync(int userId)
        {
            return await _context.Exercises.Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task<Exercise?> AddAsync(ExerciseDto exerciseDto)
        {
            var exercise = new Exercise
            {
                Name = exerciseDto.Name,
                Description = exerciseDto.Description,
                IsUsermade = exerciseDto.IsUsermade,
                UserId = exerciseDto.UserId
            };
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<Exercise?> UpdateAsync(int id, ExerciseDto exerciseDto)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return null;

            exercise.Name = exerciseDto.Name;
            exercise.Description = exerciseDto.Description;
            exercise.IsUsermade = exerciseDto.IsUsermade;
            exercise.UserId = exerciseDto.UserId;

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

        public async Task<bool> ExerciseExists(int exerciseId)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == exerciseId);
        }
    }
}