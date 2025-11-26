using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.ExerciseSession;
using api.Models;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Repositories
{
    public class ExerciseSessionRepositoryTests
    {
        #region Test Setup

        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            context.ExerciseSessions.AddRange(
                new ExerciseSession
                {
                    Id = 1,
                    ExerciseId = 1,
                    UserId = "user1",
                    Notes = "Session 1",
                    CreatedAt = DateTime.UtcNow,
                    Sets = new List<ExerciseSet>
                    {
                        new ExerciseSet
                        {
                            Id = 1,
                            ExerciseId = 1,
                            ExerciseSessionId = 1,
                            Repetitions = 10,
                            Weight = 50
                        }
                    }
                },
                new ExerciseSession
                {
                    Id = 2,
                    ExerciseId = 2,
                    UserId = "user2",
                    Notes = "Session 2",
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            return context;
        }

        #endregion

        #region Create ExerciseSession

        [Fact]
        public async Task AddAsync_CreatesExerciseSession()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var dto = new ExerciseSessionCreateDto
            {
                ExerciseId = 3,
                Notes = "New session",
                CreatedAt = DateTime.UtcNow
            };

            var result = await repo.AddAsync("user1", dto);

            Assert.NotNull(result);
            Assert.Equal("user1", result.UserId);
            Assert.Equal(3, result.ExerciseId);
        }

        #endregion

        #region Create ExerciseSet

        [Fact]
        public async Task AddSetAsync_CreatesExerciseSet()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var dto = new ExerciseSetCreateDto
            {
                ExerciseSessionId = 1,
                Repetitions = 8,
                Weight = 60
            };

            var result = await repo.AddSetAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(8, result.Repetitions);
            Assert.Equal(60, result.Weight);
        }

        #endregion

        #region Get ExerciseSessions

        [Fact]
        public async Task GetAllAsync_ReturnsUserSessionsWithSets()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var sessions = await repo.GetAllAsync("user1");

            Assert.Single(sessions);
            Assert.Single(sessions.First().Sets);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSessionWithSets()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var session = await repo.GetByIdAsync(1);

            Assert.NotNull(session);
            Assert.Single(session.Sets);
        }

        [Fact]
        public async Task GetSessionsByExerciseId_ReturnsCorrectSessions()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var sessions = await repo.GetSessionsByExerciseId(1);

            Assert.Single(sessions);
            Assert.Equal(1, sessions.First().ExerciseId);
        }

        #endregion

        #region Get ExerciseSets

        [Fact]
        public async Task GetSetByIdAsync_ReturnsSet()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var set = await repo.GetSetByIdAsync(1);

            Assert.NotNull(set);
            Assert.Equal(10, set.Repetitions);
        }

        [Fact]
        public async Task GetSetsBySessionId_ReturnsSets()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var sets = await repo.GetSetsBySessionIdAsync(1);

            Assert.Single(sets);
        }

        #endregion

        #region Exists Checks

        [Fact]
        public async Task ExerciseSessionExists_ReturnsTrue_WhenOwnedByUser()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var exists = await repo.ExerciseSessionExists("user1", 1);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExerciseSessionExists_ReturnsFalse_WhenNotOwned()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            Assert.False(await repo.ExerciseSessionExists("user1", 2));
        }

        [Fact]
        public async Task ExerciseSetExists_ReturnsTrue_WhenBelongsToUser()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var exists = await repo.ExerciseSetExists("user1", 1);

            Assert.True(exists);
        }

        #endregion

        #region Update Operations

        [Fact]
        public async Task UpdateAsync_UpdatesSessionAndSets()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var dto = new ExerciseSessionUpdateDto
            {
                ExerciseId = 99,
                Notes = "Updated notes",
                CreatedAt = DateTime.UtcNow
            };

            var result = await repo.UpdateAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(99, result.ExerciseId);

            var set = await context.ExerciseSets.FirstAsync();
            Assert.Equal(99, set.ExerciseId);
        }

        [Fact]
        public async Task UpdateSetAsync_UpdatesRepsAndWeight()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var dto = new ExerciseSetUpdateDto
            {
                Repetitions = 12,
                Weight = 70
            };

            var result = await repo.UpdateSetAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(12, result.Repetitions);
            Assert.Equal(70, result.Weight);
        }

        #endregion

        #region Delete Operations

        [Fact]
        public async Task DeleteAsync_DeletesSession()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var result = await repo.DeleteAsync(1);

            Assert.NotNull(result);
            Assert.Null(await context.ExerciseSessions.FindAsync(1));
        }

        [Fact]
        public async Task DeleteSetAsync_DeletesSet()
        {
            var context = await GetDbContext();
            var repo = new ExerciseSessionRepository(context);

            var result = await repo.DeleteSetAsync(1);

            Assert.NotNull(result);
            Assert.Null(await context.ExerciseSets.FindAsync(1));
        }

        #endregion
    }
}
