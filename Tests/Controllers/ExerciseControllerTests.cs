using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Controllers;
using Api.Dtos.Exercise;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Security.Claims;


namespace Tests.Controllers
{
    public class ExerciseControllerTests
    {
        private readonly Mock<IExerciseRepository> _exerciseRepoMock = new();
        private readonly Mock<IExerciseSessionRepository> _sessionRepoMock = new();
        private readonly Mock<ITrainingProgramRepository> _trainingRepoMock = new();
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ILogger<ExerciseController>> _loggerMock = new();

        private readonly string _testUserId = "test-user-id";

        public ExerciseControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private ExerciseController GetController()
        {
            var controller = new ExerciseController(
                _exerciseRepoMock.Object,
                _sessionRepoMock.Object,
                _trainingRepoMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _testUserId),
                    new Claim(ClaimTypes.Name, "testuser"),
                    new Claim(ClaimTypes.Email, "test@test.com")
                },
                "TestAuth"
            ));

            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = user
            };

            return controller;
        }

        #region GET /api/exercises

        [Fact]
        public async Task GetAllExercises_ReturnsOk_WithExercises()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise { Id = 1, UserId = _testUserId }
            };
            _exerciseRepoMock.Setup(r => r.GetAllAsync(_testUserId))
                             .ReturnsAsync(exercises);

            var controller = GetController();

            // Act
            var result = await controller.GetAllExercises();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetAllExercises_ReturnsOkWithEmptyList_WhenNoExercises()
        {
            // Arrange
            _exerciseRepoMock
                .Setup(r => r.GetAllAsync(_testUserId))
                .ReturnsAsync(new List<Exercise>());

            var controller = GetController();

            // Act
            var result = await controller.GetAllExercises();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var exercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
            Assert.Empty(exercises);
        }

        #endregion

        #region GET /api/exercises/{id}

        [Fact]
        public async Task GetExerciseById_ReturnsOk_WhenExerciseExists()
        {
            var exercise = new Exercise { Id = 1, UserId = _testUserId };
            _exerciseRepoMock.Setup(r => r.GetByIdAsync(_testUserId, 1))
                             .ReturnsAsync(exercise);

            var controller = GetController();
            var result = await controller.GetExerciseById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ExerciseDto>(okResult.Value);
            Assert.Equal(1, returned.Id);
        }

        [Fact]
        public async Task GetExerciseById_ReturnsNotFound_WhenExerciseDoesNotExist()
        {
            _exerciseRepoMock.Setup(r => r.GetByIdAsync(_testUserId, 1))
                             .ReturnsAsync((Exercise)null);

            var controller = GetController();
            var result = await controller.GetExerciseById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region POST /api/exercises/create

        [Fact]
        public async Task CreateExercise_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new ExerciseCreateDto { Name = "Pushup", Description = "Chest exercise" };
            var createdExercise = new Exercise { Id = 1, UserId = _testUserId, Name = createDto.Name };

            _exerciseRepoMock.Setup(r => r.AddAsync(_testUserId, createDto))
                             .ReturnsAsync(createdExercise);

            var controller = GetController();
            var result = await controller.CreateExercise(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returned = Assert.IsType<Exercise>(createdResult.Value);
            Assert.Equal(1, returned.Id);
        }

        [Fact]
        public async Task CreateExercise_ReturnsBadRequest_WhenCreationFails()
        {
            var createDto = new ExerciseCreateDto { Name = "Pushup" };
            _exerciseRepoMock.Setup(r => r.AddAsync(_testUserId, createDto))
                             .ReturnsAsync((Exercise)null);

            var controller = GetController();
            var result = await controller.CreateExercise(createDto);

            Assert.IsType<BadRequestResult>(result);
        }

        #endregion

        #region PUT /api/exercises/update/{id}

        [Fact]
        public async Task UpdateExercise_ReturnsNoContent_WhenSuccessful()
        {
            var updateDto = new ExerciseUpdateDto { Name = "Pullup", Description = "Back exercise" };
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1))
                             .ReturnsAsync(true);
            _exerciseRepoMock.Setup(r => r.UpdateAsync(1, updateDto))
                             .ReturnsAsync(new Exercise { Id = 1 });

            var controller = GetController();
            var result = await controller.UpdateExercise(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateExercise_ReturnsNotFound_WhenExerciseDoesNotExist()
        {
            var updateDto = new ExerciseUpdateDto { Name = "Pullup" };
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1))
                             .ReturnsAsync(false);

            var controller = GetController();
            var result = await controller.UpdateExercise(1, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region DELETE /api/exercises/delete/{id}

        [Fact]
        public async Task DeleteExercise_ReturnsNoContent_WhenSuccessful()
        {
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1)).ReturnsAsync(true);
            _sessionRepoMock.Setup(r => r.GetSessionsByExerciseId(1)).ReturnsAsync(new List<ExerciseSession>());
            _trainingRepoMock.Setup(r => r.GetExercisesByExerciseId(1)).ReturnsAsync(new List<ProgrammedExercise>());
            _exerciseRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(new Exercise { Id = 1 });

            var controller = GetController();
            var result = await controller.DeleteExercise(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteExercise_ReturnsBadRequest_WhenSessionsExist()
        {
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1)).ReturnsAsync(true);
            _sessionRepoMock.Setup(r => r.GetSessionsByExerciseId(1))
                            .ReturnsAsync(new List<ExerciseSession> { new ExerciseSession{ ExerciseId = 1 , UserId = _testUserId} });

            var controller = GetController();
            var result = await controller.DeleteExercise(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("existing exercise sessions", badRequest.Value.ToString());
        }

        [Fact]
        public async Task DeleteExercise_ReturnsBadRequest_WhenProgrammedExercisesExist()
        {
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1)).ReturnsAsync(true);
            _sessionRepoMock.Setup(r => r.GetSessionsByExerciseId(1)).ReturnsAsync(new List<ExerciseSession>());
            _trainingRepoMock.Setup(r => r.GetExercisesByExerciseId(1)).ReturnsAsync(new List<ProgrammedExercise> { new ProgrammedExercise() });

            var controller = GetController();
            var result = await controller.DeleteExercise(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("existing programmed exercises", badRequest.Value.ToString());
        }

        [Fact]
        public async Task DeleteExercise_ReturnsNotFound_WhenExerciseDoesNotExist()
        {
            _exerciseRepoMock.Setup(r => r.ExerciseExists(_testUserId, 1)).ReturnsAsync(false);

            var controller = GetController();
            var result = await controller.DeleteExercise(1);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}
