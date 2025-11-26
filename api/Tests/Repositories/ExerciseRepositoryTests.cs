using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Exercise;
using api.Models;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Repositories
{
    public class ExerciseRepositoryTests
    {
        #region Helpers

        private async Task<ApplicationDbContext> GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed test data
            context.Exercises.AddRange(
                new Exercise { Id = 1, Name = "Pushup", UserId = "user1", IsUsermade = true },
                new Exercise { Id = 2, Name = "Pullup", UserId = "user2", IsUsermade = true },
                new Exercise { Id = 3, Name = "Squat", UserId = null, IsUsermade = false } // shared exercise
            );

            await context.SaveChangesAsync();
            return context;
        }

        #endregion

        #region Get Methods

        [Fact]
        public async Task GetAllAsync_ReturnsUserAndSharedExercises()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.GetAllAsync("user1");

            Assert.Equal(2, result.Count); // user1 + shared exercise
            Assert.Contains(result, e => e.Name == "Pushup");
            Assert.Contains(result, e => e.Name == "Squat");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsExerciseForUser()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.GetByIdAsync("user1", 1);
            Assert.NotNull(result);
            Assert.Equal("Pushup", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSharedExercise_WhenUserIdDoesNotMatch()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.GetByIdAsync("user1", 3);
            Assert.NotNull(result);
            Assert.Equal("Squat", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenExerciseDoesNotExist()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.GetByIdAsync("user1", 999);
            Assert.Null(result);
        }

        #endregion

        #region Add Methods

        [Fact]
        public async Task AddAsync_AddsNewExercise()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            var repo = new ExerciseRepository(context);

            var dto = new ExerciseCreateDto
            {
                Name = "Bench Press",
                Description = "Chest exercise"
            };

            var result = await repo.AddAsync("user1", dto);

            Assert.NotNull(result);
            Assert.Equal("Bench Press", result.Name);
            Assert.Equal("user1", result.UserId);

            var dbExercise = await context.Exercises.FindAsync(result.Id);
            Assert.NotNull(dbExercise);
        }

        #endregion

        #region Update Methods

        [Fact]
        public async Task UpdateAsync_UpdatesExercise()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var updateDto = new ExerciseUpdateDto
            {
                Name = "Modified Pushup",
                Description = "Updated description"
            };

            var result = await repo.UpdateAsync(1, updateDto);

            Assert.NotNull(result);
            Assert.Equal("Modified Pushup", result.Name);

            var dbExercise = await context.Exercises.FindAsync(1);
            Assert.Equal("Modified Pushup", dbExercise.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenExerciseDoesNotExist()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var updateDto = new ExerciseUpdateDto
            {
                Name = "Nonexistent",
                Description = "Nope"
            };

            var result = await repo.UpdateAsync(999, updateDto);
            Assert.Null(result);
        }

        #endregion

        #region Delete Methods

        [Fact]
        public async Task DeleteAsync_DeletesExercise()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.DeleteAsync(1);

            Assert.NotNull(result);

            var dbExercise = await context.Exercises.FindAsync(1);
            Assert.Null(dbExercise);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNull_WhenExerciseDoesNotExist()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var result = await repo.DeleteAsync(999);
            Assert.Null(result);
        }

        #endregion

        #region Exists Methods

        [Fact]
        public async Task ExerciseExists_ReturnsTrue_WhenExists()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var exists = await repo.ExerciseExists("user1", 1);
            Assert.True(exists);
        }

        [Fact]
        public async Task ExerciseExists_ReturnsFalse_WhenDoesNotExist()
        {
            var context = await GetDbContextWithData();
            var repo = new ExerciseRepository(context);

            var exists = await repo.ExerciseExists("user1", 999);
            Assert.False(exists);
        }

        #endregion
    }
}
