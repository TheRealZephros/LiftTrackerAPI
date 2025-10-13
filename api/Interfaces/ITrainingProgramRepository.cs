using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.TrainingProgram;
using api.Models;

namespace api.Interfaces
{
    public interface ITrainingProgramRepository
    {
        Task<bool> TrainingProgramExists(int programId);
        Task<bool> ProgramDayExists(int dayId);
        Task<bool> ProgrammedExerciseExists(int id);
        Task<bool> ProgrammedExercisePositionExists(int programDayId, int position);
        Task<List<TrainingProgram>?> GetTrainingProgramsForUser(string userId);
        Task<TrainingProgram?> GetTrainingProgramById(int programId);
        Task<List<ProgramDay>?> GetDaysByProgramId(int programId);
        Task<ProgramDay?> GetDayById(int dayId);
        Task<List<ProgrammedExercise>?> GetExercisesByDay(int dayId);
        Task<ProgrammedExercise?> GetExerciseById(int id);
        Task<List<ProgrammedExercise>?> GetExercisesByExerciseId(int exerciseId);
        Task<TrainingProgram?> CreateTrainingProgram(TrainingProgramCreateDto program);
        Task<ProgramDay?> CreateProgramDay(ProgramDayCreateDto programDay);
        Task<ProgrammedExercise?> CreateProgrammedExercise(ProgrammedExerciseCreateDto programmedExercise);
        Task<TrainingProgram?> UpdateTrainingProgram(int id, TrainingProgramUpdateDto program);
        Task<ProgramDay?> UpdateProgramDay(int id, ProgramDayUpdateDto programDay);
        Task<ProgrammedExercise?> UpdateProgrammedExercise(int id, ProgrammedExerciseUpdateDto programmedExercise);
        Task<TrainingProgram?> DeleteTrainingProgram(int programId);
        Task<ProgramDay?> DeleteProgramDay(int dayId);
        Task<ProgrammedExercise?> DeleteProgrammedExercise(int id);
    }
}