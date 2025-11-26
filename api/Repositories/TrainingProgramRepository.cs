using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Interfaces;
using api.Models;
using api.Dtos.TrainingProgram;

namespace api.Repositories
{
    public class TrainingProgramRepository : ITrainingProgramRepository
    {
        private readonly ApplicationDbContext _context;
        public TrainingProgramRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProgramDay?> CreateProgramDay(ProgramDayCreateDto dto)
        {
            if (dto == null) return null;
            var programDay = new ProgramDay
            {
                TrainingProgramId = dto.TrainingProgramId,
                Position = dto.Position,
                Name = dto.Name
            };
            _context.ProgramDays.Add(programDay);
            await _context.SaveChangesAsync();
            return programDay;
        }

        public async Task<ProgrammedExercise?> CreateProgrammedExercise(ProgrammedExerciseCreateDto dto)
        {
            if (dto == null) return null;
            
            var programmedExercise = new ProgrammedExercise
            {
                ProgramDayId = dto.ProgramDayId,
                ExerciseId = dto.ExerciseId,
                Position = dto.Position,
                Sets = dto.Sets,
                Reps = dto.Reps,
            };
            _context.ProgrammedExercises.Add(programmedExercise);
            await _context.SaveChangesAsync();
            return programmedExercise;
        }

        public async Task<TrainingProgram?> CreateTrainingProgram(string userId, TrainingProgramCreateDto dto)
        {
            if (dto == null) return null;
            var trainingProgram = new TrainingProgram
            {
                Name = dto.Name,
                Description = dto.Description,
                UserId = userId
            };
            _context.TrainingPrograms.Add(trainingProgram);
            await _context.SaveChangesAsync();
            return trainingProgram;
        }

        public async Task<ProgramDay?> DeleteProgramDay(int dayId)
        {
            var programDay = await _context.ProgramDays.FindAsync(dayId);
            if (programDay == null)
            {
                return null;
            }

            _context.ProgramDays.Remove(programDay);
            await _context.SaveChangesAsync();
            return programDay;
        }

        public async Task<ProgrammedExercise?> DeleteProgrammedExercise(int id)
        {
            var programmedExercise = await _context.ProgrammedExercises.FindAsync(id);
            if (programmedExercise == null)
            {
                return null;
            }
            _context.ProgrammedExercises.Remove(programmedExercise);
            await _context.SaveChangesAsync();
            return programmedExercise;

        }

        public async Task<TrainingProgram?> DeleteTrainingProgram(int programId)
        {
            var trainingProgram = await _context.TrainingPrograms.FindAsync(programId);
            if (trainingProgram == null)
            {
                return null;
            }

            _context.TrainingPrograms.Remove(trainingProgram);
            await _context.SaveChangesAsync();
            return trainingProgram;
            
        }

        public async Task<ProgramDay?> GetDayById(int dayId)
        {
            return await _context.ProgramDays
            .Include(d => d.Exercises)
            .FirstOrDefaultAsync(d => d.Id == dayId);
        }

        public async Task<List<ProgramDay>?> GetDaysByProgramId(int programId)
        {
            Console.WriteLine("Getting days for programId: " + programId);
            Console.WriteLine("Context ProgramDays count: " + _context.ProgramDays.Count());

            return await _context.ProgramDays
                .Where(d => d.TrainingProgramId == programId)
                .OrderBy(d => d.Position)
                .Include(d => d.Exercises)
                .ToListAsync();
        }

        public async Task<ProgrammedExercise?> GetExerciseById(int id)
        {
            return await _context.ProgrammedExercises.FindAsync(id);
        }

        public async Task<List<ProgrammedExercise>?> GetExercisesByDay(int dayId)
        {
            return await _context.ProgrammedExercises
                .Where(e => e.ProgramDayId == dayId)
                .OrderBy(e => e.Position)
                .ToListAsync();
        }

        public async Task<List<ProgrammedExercise>?> GetExercisesByExerciseId(int exerciseId)
        {
            return await _context.ProgrammedExercises
                .Where(e => e.ExerciseId == exerciseId)
                .OrderBy(e => e.Position)
                .ToListAsync();
        }

        public async Task<TrainingProgram?> GetTrainingProgramById(int programId)
        {
            return await _context.TrainingPrograms
                .Include(p => p.Days)
                .ThenInclude(d => d.Exercises)
                .FirstOrDefaultAsync(p => p.Id == programId);
        }

        public async Task<List<TrainingProgram>?> GetTrainingProgramsForUser(string userId)
        {
            var programs = await _context.TrainingPrograms
                .Include(p => p.Days)
                .ThenInclude(d => d.Exercises)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
           return programs;
        }

        public async Task<bool> ProgramDayExists(string userId, int dayId)
        {
            return await _context.ProgramDays.AnyAsync(d => d.Id == dayId && d.TrainingProgram.UserId == userId);
        }

        public async Task<bool> ProgrammedExerciseExists(string userId, int id)
        {
            return await _context.ProgrammedExercises.AnyAsync(e => e.Id == id && e.ProgramDay.TrainingProgram.UserId == userId);
        }

        public Task<bool> ProgrammedExercisePositionExists(string userId, int programDayId, int position)
        {
            return _context.ProgrammedExercises.AnyAsync(e =>
                e.ProgramDayId == programDayId
                && e.Position == position &&
                e.ProgramDay.TrainingProgram.UserId == userId);
        }

        public async Task<bool> TrainingProgramExists(string userId, int programId)
        {
            return await _context.TrainingPrograms.AnyAsync(p => p.Id == programId && p.UserId == userId);
        }

        public async Task<ProgramDay?> UpdateProgramDay(int id, ProgramDayUpdateDto dto)
        {
            if (dto == null) return null;

            var programDay = await _context.ProgramDays.FindAsync(id);
            if (programDay == null) return null;

            programDay.Name = dto.Name;
            programDay.Position = dto.Position;
            programDay.Description = dto.Description;
            programDay.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return programDay;
        }

        public async Task<ProgrammedExercise?> UpdateProgrammedExercise(int id,ProgrammedExerciseUpdateDto dto)
        {
            if (dto == null) return null;

            var programmedExercise = await _context.ProgrammedExercises.FindAsync(id);
            if (programmedExercise == null) return null;

            programmedExercise.Position = dto.Position;
            programmedExercise.Sets = dto.Sets;
            programmedExercise.Reps = dto.Reps;
            programmedExercise.RestTime = dto.RestTime;
            programmedExercise.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return programmedExercise;
        }

        public async Task<TrainingProgram?> UpdateTrainingProgram(int id, TrainingProgramUpdateDto dto)
        {
            var existingProgram = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == id);
            if (existingProgram == null)
            {
                return null;
            }
            existingProgram.Name = dto.Name;
            existingProgram.Description = dto.Description;
            existingProgram.IsWeekDaySynced = dto.IsWeekDaySynced;

            _context.SaveChanges();
            return existingProgram;
        }
    }
}