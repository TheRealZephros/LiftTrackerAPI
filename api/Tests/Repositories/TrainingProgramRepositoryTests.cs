using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.TrainingProgram;
using api.Models;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Repositories
{
    public class TrainingProgramRepositoryTests
    {
        #region Test Setup

        private async Task<ApplicationDbContext> GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            var program = new TrainingProgram
            {
                Id = 1,
                Name = "Push Pull Legs",
                UserId = "user1",
                CreatedAt = DateTime.UtcNow
            };

            var day = new ProgramDay
            {
                Id = 1,
                TrainingProgramId = 1,
                Name = "Push Day",
                Position = 1
            };

            var exercise = new ProgrammedExercise
            {
                Id = 1,
                ProgramDayId = 1,
                ExerciseId = 10,
                Position = 1,
                Sets = 3,
                Reps = 10
            };

            context.TrainingPrograms.Add(program);
            context.ProgramDays.Add(day);
            context.ProgrammedExercises.Add(exercise);

            await context.SaveChangesAsync();
            return context;
        }

        #endregion

        #region CreateTrainingProgram

        [Fact]
        public async Task CreateTrainingProgram_CreatesProgram()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new TrainingProgramCreateDto
            {
                Name = "Upper Lower",
                Description = "Split program"
            };

            var result = await repo.CreateTrainingProgram("user2", dto);

            Assert.NotNull(result);
            Assert.Equal("user2", result.UserId);
            Assert.Equal("Upper Lower", result.Name);
        }

        #endregion

        #region CreateProgramDay

        [Fact]
        public async Task CreateProgramDay_CreatesDay()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new ProgramDayCreateDto
            {
                TrainingProgramId = 1,
                Name = "Pull Day",
                Position = 2
            };

            var result = await repo.CreateProgramDay(dto);

            Assert.NotNull(result);
            Assert.Equal(2, result.Position);
        }

        #endregion

        #region CreateProgrammedExercise

        [Fact]
        public async Task CreateProgrammedExercise_CreatesExercise()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new ProgrammedExerciseCreateDto
            {
                ProgramDayId = 1,
                ExerciseId = 99,
                Position = 2,
                Sets = 4,
                Reps = 12
            };

            var result = await repo.CreateProgrammedExercise(dto);

            Assert.NotNull(result);
            Assert.Equal(99, result.ExerciseId);
        }

        #endregion

        #region GetTrainingProgram

        [Fact]
        public async Task GetTrainingProgramById_ReturnsNestedData()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var program = await repo.GetTrainingProgramById(1);

            Assert.NotNull(program);
            Assert.Single(program.Days);
            Assert.Single(program.Days.First().Exercises);
        }

        [Fact]
        public async Task GetTrainingProgramsForUser_ReturnsUserPrograms()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var programs = await repo.GetTrainingProgramsForUser("user1");

            Assert.Single(programs);
            Assert.Equal("Push Pull Legs", programs.First().Name);
        }

        #endregion

        #region ProgramDays Queries

        [Fact]
        public async Task GetDaysByProgramId_ReturnsOrderedDays()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var days = await repo.GetDaysByProgramId(1);

            Assert.Single(days);
            Assert.Equal("Push Day", days.First().Name);
        }

        #endregion

        #region ProgrammedExercises Queries

        [Fact]
        public async Task GetExercisesByDay_ReturnsOrderedExercises()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var exercises = await repo.GetExercisesByDay(1);

            Assert.Single(exercises);
            Assert.Equal(10, exercises.First().ExerciseId);
        }

        [Fact]
        public async Task GetExercisesByExerciseId_ReturnsMatchingExercises()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var exercises = await repo.GetExercisesByExerciseId(10);

            Assert.Single(exercises);
        }

        #endregion

        #region Exists Checks

        [Fact]
        public async Task ProgramDayExists_ReturnsTrue_WhenOwnedByUser()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            Assert.True(await repo.ProgramDayExists("user1", 1));
        }

        [Fact]
        public async Task ProgrammedExerciseExists_ReturnsTrue_WhenOwnedByUser()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            Assert.True(await repo.ProgrammedExerciseExists("user1", 1));
        }

        [Fact]
        public async Task ProgrammedExercisePositionExists_ReturnsTrue_WhenPositionTaken()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var exists = await repo.ProgrammedExercisePositionExists("user1", 1, 1);

            Assert.True(exists);
        }

        [Fact]
        public async Task TrainingProgramExists_ReturnsTrue_WhenOwned()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            Assert.True(await repo.TrainingProgramExists("user1", 1));
        }

        #endregion

        #region Update Operations

        [Fact]
        public async Task UpdateProgramDay_UpdatesFields()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new ProgramDayUpdateDto
            {
                Name = "Updated Day",
                Position = 3
            };

            var result = await repo.UpdateProgramDay(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Updated Day", result.Name);
        }

        [Fact]
        public async Task UpdateProgrammedExercise_UpdatesExercise()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new ProgrammedExerciseUpdateDto
            {
                Position = 2,
                Sets = 5,
                Reps = 15
            };

            var result = await repo.UpdateProgrammedExercise(1, dto);

            Assert.NotNull(result);
            Assert.Equal(5, result.Sets);
        }

        [Fact]
        public async Task UpdateTrainingProgram_UpdatesProgram()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var dto = new TrainingProgramUpdateDto
            {
                Name = "Updated Program",
                Description = "Updated description",
                IsWeekDaySynced = true
            };

            var result = await repo.UpdateTrainingProgram(1, dto);

            Assert.NotNull(result);
            Assert.Equal("Updated Program", result.Name);
        }

        #endregion

        #region Delete Operations

        [Fact]
        public async Task DeleteTrainingProgram_DeletesProgram()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var result = await repo.DeleteTrainingProgram(1);

            Assert.NotNull(result);
            Assert.Null(await context.TrainingPrograms.FindAsync(1));
        }

        [Fact]
        public async Task DeleteProgramDay_DeletesDay()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var result = await repo.DeleteProgramDay(1);

            Assert.NotNull(result);
            Assert.Null(await context.ProgramDays.FindAsync(1));
        }

        [Fact]
        public async Task DeleteProgrammedExercise_DeletesExercise()
        {
            var context = await GetDbContextWithData();
            var repo = new TrainingProgramRepository(context);

            var result = await repo.DeleteProgrammedExercise(1);

            Assert.NotNull(result);
            Assert.Null(await context.ProgrammedExercises.FindAsync(1));
        }

        #endregion
    }
}
