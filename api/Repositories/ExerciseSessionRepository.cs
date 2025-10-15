using System;
using System.Collections.Generic;
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
        private readonly ApplicationDBContext _context;
        public ExerciseSessionRepository(ApplicationDBContext context)
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
                CreatedAt = DateTime.UtcNow,
            };
            // Add exerciseSession to the database
            await _context.ExerciseSessions.AddAsync(exerciseSession);
            await _context.SaveChangesAsync();
            return exerciseSession;
        }

        public Task<ExerciseSet?> AddSetAsync(ExerciseSetCreateDto exerciseSetDto)
        {
            var exerciseSet = new ExerciseSet
            {
                ExerciseId = exerciseSetDto.ExerciseId,
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
            return await _context.ExerciseSessions.Where(s => s.UserId == userId).ToListAsync();
        }

        public async Task<ExerciseSession?> GetByIdAsync(int id)
        {
            return await _context.ExerciseSessions.FindAsync(id);
        }

        public async Task<List<ExerciseSession>> GetSessionsByExerciseId(int exerciseId)
        {
            return await _context.ExerciseSessions.Where(s => s.ExerciseId == exerciseId).ToListAsync();
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

        public async Task<ExerciseSession?> UpdateAsync(int id, ExerciseSessionDto exerciseSessionDto)
        {
            var session = await _context.ExerciseSessions.FindAsync(id);
            if (session == null) return null;

            session.ExerciseId = exerciseSessionDto.ExerciseId;
            session.UserId = exerciseSessionDto.UserId;
            session.Notes = exerciseSessionDto.Notes;

            _context.ExerciseSessions.Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ExerciseSet?> UpdateSetAsync(int id, ExerciseSetDto exerciseSetDto)
        {
            var set = await _context.ExerciseSets.FindAsync(id);
            if (set == null) return null;
            set.ExerciseId = exerciseSetDto.ExerciseId;
            set.ExerciseSessionId = exerciseSetDto.ExerciseSessionId;
            set.Repetitions = exerciseSetDto.Repetitions;
            set.Weight = exerciseSetDto.Weight;
            _context.ExerciseSets.Update(set);
            _context.SaveChanges();
            return set;
        }
    }
}