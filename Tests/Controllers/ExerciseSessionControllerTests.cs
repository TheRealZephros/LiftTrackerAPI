using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Dtos.ExerciseSession;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Controllers
{
    public class ExerciseSessionControllerTests
    {
        #region Helpers

        private static ExerciseSessionController CreateController(
            Mock<IExerciseSessionRepository> sessionRepo,
            Mock<IExerciseRepository> exerciseRepo,
            string userId = "user1")
        {
            var userStore = new Mock<IUserStore<User>>();
            var userManager = new UserManager<User>(
                userStore.Object,
                null, null, null, null, null, null, null, null
            );

            var logger = Mock.Of<ILogger<ExerciseSessionController>>();

            var controller = new ExerciseSessionController(
                sessionRepo.Object,
                exerciseRepo.Object,
                userManager,
                logger
            );

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                    "TestAuth"
                )
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        private static ExerciseSession CreateSession(string userId = "user1")
        {
            return new ExerciseSession
            {
                Id = 1,
                UserId = userId,
                ExerciseId = 1,
                Notes = "Test Session",
                CreatedAt = DateTime.UtcNow,
                Sets = new List<ExerciseSet>()
            };
        }

        #endregion

        #region GET Sessions

        [Fact]
        public async Task GetExerciseSessionsForUser_ReturnsOk()
        {
            var repo = new Mock<IExerciseSessionRepository>();
            repo.Setup(r => r.GetAllAsync("user1"))
                .ReturnsAsync(new List<ExerciseSession> { CreateSession() });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetExerciseSessionsForUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetExerciseSessionById_ReturnsNotFound_WhenWrongUser()
        {
            var repo = new Mock<IExerciseSessionRepository>();
            repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(CreateSession("otherUser"));

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetExerciseSessionById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region GET Sets

        [Fact]
        public async Task GetExerciseSetsForSession_ReturnsOk()
        {
            var repo = new Mock<IExerciseSessionRepository>();

            repo.Setup(r => r.ExerciseSessionExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetSetsBySessionIdAsync(1))
                .ReturnsAsync(new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 1, Repetitions = 10, Weight = 50, ExerciseId = 1, ExerciseSessionId = 1 }
                });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetExerciseSetsForSession(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        #endregion

        #region POST Session

        [Fact]
        public async Task CreateExerciseSession_ReturnsCreated()
        {
            var sessionRepo = new Mock<IExerciseSessionRepository>();
            var exerciseRepo = new Mock<IExerciseRepository>();

            exerciseRepo.Setup(r => r.GetByIdAsync("user1", 1))
                .ReturnsAsync(new Exercise { Id = 1, UserId = "user1" });

            sessionRepo.Setup(r => r.AddAsync("user1", It.IsAny<ExerciseSessionCreateDto>()))
                .ReturnsAsync(CreateSession());

            var controller = CreateController(sessionRepo, exerciseRepo);

            var dto = new ExerciseSessionCreateDto
            {
                ExerciseId = 1,
                Notes = "New session",
                CreatedAt = DateTime.UtcNow
            };

            var result = await controller.CreateExerciseSession(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(created.Value);
        }

        #endregion

        #region POST Set

        [Fact]
        public async Task CreateExerciseSet_ReturnsCreated()
        {
            var repo = new Mock<IExerciseSessionRepository>();

            repo.Setup(r => r.ExerciseSessionExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(CreateSession());

            repo.Setup(r => r.AddSetAsync(1, It.IsAny<ExerciseSetCreateDto>()))
                .ReturnsAsync(new ExerciseSet { Id = 1, Repetitions = 8, Weight = 60, ExerciseSessionId = 1, ExerciseId = 1 });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var dto = new ExerciseSetCreateDto
            {
                ExerciseSessionId = 1,
                Repetitions = 8,
                Weight = 60
            };

            var result = await controller.CreateExerciseSet(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(created.Value);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task UpdateExerciseSession_ReturnsOk()
        {
            var repo = new Mock<IExerciseSessionRepository>();

            repo.Setup(r => r.ExerciseSessionExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.UpdateAsync(1, It.IsAny<ExerciseSessionUpdateDto>()))
                .ReturnsAsync(CreateSession());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.UpdateExerciseSession(1, new ExerciseSessionUpdateDto());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task DeleteExerciseSession_ReturnsNoContent()
        {
            var repo = new Mock<IExerciseSessionRepository>();

            repo.Setup(r => r.ExerciseSessionExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(CreateSession());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.DeleteExerciseSession(1);

            Assert.IsType<NoContentResult>(result);
        }

        #endregion
    }
}
