using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.ExerciseSession;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ExerciseSessionRepository : IExerciseSessionRepository
    {
        private readonly ApplicationDbContext _context;
        public ExerciseSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ExerciseSession?> AddAsync(string userId, ExerciseSessionCreateDto exerciseSessionDto)
        {
            var exerciseSession = new ExerciseSession
            {
                ExerciseId = exerciseSessionDto.ExerciseId,
                UserId = userId,
                Notes = exerciseSessionDto.Notes,
                CreatedAt = exerciseSessionDto.CreatedAt
            };
            // Add exerciseSession to the database
            await _context.ExerciseSessions.AddAsync(exerciseSession);
            await _context.SaveChangesAsync();
            return exerciseSession;
        }

        public Task<ExerciseSet?> AddSetAsync(int exerciseId, ExerciseSetCreateDto exerciseSetDto)
        {
            var exerciseSet = new ExerciseSet
            {
                ExerciseId = exerciseId,
                ExerciseSessionId = exerciseSetDto.ExerciseSessionId,
                Repetitions = exerciseSetDto.Repetitions,
                Weight = exerciseSetDto.Weight
            };
            _context.ExerciseSets.Add(exerciseSet);
            _context.SaveChanges();
            return Task.FromResult<ExerciseSet?>(exerciseSet);
        }

        public async Task<ExerciseSession?> DeleteAsync(int id)
        {
            var session = await _context.ExerciseSessions.FindAsync(id);
            if (session == null)
            {
                return null;
            }

            _context.ExerciseSessions.Remove(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ExerciseSet?> DeleteSetAsync(int id)
        {
            var set = await _context.ExerciseSets.FindAsync(id);
            if (set == null)
            {
                return null;
            }
            _context.ExerciseSets.Remove(set);
            await _context.SaveChangesAsync();
            return set;
        }

        public async Task<bool> ExerciseSessionExists(string userId, int sessionId)
        {
            return await _context.ExerciseSessions.AnyAsync(s => s.Id == sessionId && s.UserId == userId);
        }

        public async Task<bool> ExerciseSetExists(string userId, int setId)
        {
            return await _context.ExerciseSets.AnyAsync(s => s.Id == setId && s.ExerciseSession.UserId == userId);
        }

        public async Task<List<ExerciseSession>> GetAllAsync(string userId)
        {
            return (
                await _context.ExerciseSessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Sets)
                .ToListAsync()
            );
        }

        public async Task<ExerciseSession?> GetByIdAsync(int id)
        {
            return await _context.ExerciseSessions
            .Include(s => s.Sets)
            .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ExerciseSession>> GetSessionsByExerciseId(int exerciseId)
        {
            return await _context.ExerciseSessions
            .Where(s => s.ExerciseId == exerciseId)
            .Include(s => s.Sets)
            .ToListAsync();
        }

        public async Task<ExerciseSet?> GetSetByIdAsync(int setId)
        {
            return await _context.ExerciseSets.FindAsync(setId);
        }

        public async Task<List<ExerciseSet>?> GetSetsByExerciseId(int exerciseId)
        {
            return await _context.ExerciseSets.Where(s => s.ExerciseId == exerciseId).ToListAsync();
        }

        public async Task<List<ExerciseSet>> GetSetsBySessionIdAsync(int sessionId)
        {
            return await _context.ExerciseSets.Where(s => s.ExerciseSessionId == sessionId).ToListAsync();
        }

        public async Task<ExerciseSession?> UpdateAsync(int id, ExerciseSessionUpdateDto exerciseSessionDto)
        {
            var session = await _context.ExerciseSessions.FindAsync(id);
            if (session == null) return null;

            session.ExerciseId = exerciseSessionDto.ExerciseId;
            session.Notes = exerciseSessionDto.Notes;
            session.CreatedAt = exerciseSessionDto.CreatedAt;

            _context.ExerciseSessions.Update(session);

            var sets = await _context.ExerciseSets.Where(s => s.ExerciseSessionId == id).ToListAsync();
            foreach (var set in sets)
            {
                set.ExerciseId = exerciseSessionDto.ExerciseId;
                _context.ExerciseSets.Update(set);
            }

            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ExerciseSet?> UpdateSetAsync(int id, ExerciseSetUpdateDto exerciseSetDto)
        {
            var set = await _context.ExerciseSets.FindAsync(id);
            if (set == null) return null;

            set.Repetitions = exerciseSetDto.Repetitions;
            set.Weight = exerciseSetDto.Weight;

            _context.ExerciseSets.Update(set);
            await _context.SaveChangesAsync();
            return set;
        }
    }
}