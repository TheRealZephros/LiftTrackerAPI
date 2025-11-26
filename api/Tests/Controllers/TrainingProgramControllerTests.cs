using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Controllers;
using api.Dtos.TrainingProgram;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace api.Tests.Controllers
{
    public class TrainingProgramControllerTests
    {
        #region Helpers

        private static TrainingProgramController CreateController(
            Mock<ITrainingProgramRepository> programRepo,
            Mock<IExerciseRepository> exerciseRepo,
            string userId = "user1")
        {
            var userStore = new Mock<IUserStore<User>>();
            var userManager = new UserManager<User>(
                userStore.Object,
                null, null, null, null, null, null, null, null
            );

            var logger = Mock.Of<ILogger<TrainingProgramController>>();

            var controller = new TrainingProgramController(
                exerciseRepo.Object,
                programRepo.Object,
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

        private static TrainingProgram CreateProgram(string userId = "user1")
        {
            return new TrainingProgram
            {
                Id = 1,
                UserId = userId,
                Name = "Test Program",
                Description = "Description",
                CreatedAt = DateTime.UtcNow,
                Days = new List<ProgramDay>()
            };
        }

        #endregion

        #region GET Programs

        [Fact]
        public async Task GetTrainingProgramsForUser_ReturnsOk()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            repo.Setup(r => r.GetTrainingProgramsForUser("user1"))
                .ReturnsAsync(new List<TrainingProgram> { CreateProgram() });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetTrainingProgramsForUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            var values = Assert.IsAssignableFrom<IEnumerable<TrainingProgramGetAllDto>>(ok.Value);
            Assert.Single(values);
        }

        [Fact]
        public async Task GetTrainingProgramById_ReturnsNotFound_WhenWrongUser()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            repo.Setup(r => r.GetTrainingProgramById(1))
                .ReturnsAsync(CreateProgram("otherUser"));

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetTrainingProgramById(1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetTrainingProgramsForUser_ReturnsNotFound_WhenRepoReturnsNull()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            repo.Setup(r => r.GetTrainingProgramsForUser("user1"))
                .ReturnsAsync((IEnumerable<TrainingProgram>)null!);

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetTrainingProgramsForUser();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTrainingProgramsForUser_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            controller.ModelState.AddModelError("error", "invalid");

            var result = await controller.GetTrainingProgramsForUser();

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetDaysByProgramId_ReturnsNotFound_WhenDaysNull()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);
            repo.Setup(r => r.GetDaysByProgramId(1))
                .ReturnsAsync((List<ProgramDay>)null!);

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetDaysByProgramId(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDayById_ReturnsNotFound_WhenProgramDoesNotBelongToUser()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.GetDayById(1))
                .ReturnsAsync(new ProgramDay { Id = 1, TrainingProgramId = 99 });

            repo.Setup(r => r.TrainingProgramExists("user1", 99))
                .ReturnsAsync(false);

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetDayById(1);

            Assert.IsType<NotFoundResult>(result);
        }


        #endregion

        #region GET Days & Exercises

        [Fact]
        public async Task GetDaysByProgramId_ReturnsOk()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetDaysByProgramId(1))
                .ReturnsAsync(new List<ProgramDay>
                {
                    new ProgramDay { Id = 1, Name = "Day 1", Position = 1 }
                });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetDaysByProgramId(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetExercisesByDay_ReturnsOk()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.ProgramDayExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetExercisesByDay(1))
                .ReturnsAsync(new List<ProgrammedExercise>
                {
                    new ProgrammedExercise { Id = 1, ExerciseId = 1, Position = 1 }
                });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.GetExercisesByDay(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task CreateProgrammedExercise_ReturnsBadRequest_WhenPositionExists()
        {
            var repo = new Mock<ITrainingProgramRepository>();
            var exerciseRepo = new Mock<IExerciseRepository>();

            repo.Setup(r => r.ProgramDayExists("user1", 1)).ReturnsAsync(true);
            exerciseRepo.Setup(r => r.ExerciseExists("user1", 1)).ReturnsAsync(true);
            repo.Setup(r => r.ProgrammedExercisePositionExists("user1", 1, 1))
                .ReturnsAsync(true);

            var controller = CreateController(repo, exerciseRepo);

            var dto = new ProgrammedExerciseCreateDto
            {
                ProgramDayId = 1,
                ExerciseId = 1,
                Position = 1
            };

            var result = await controller.CreateProgrammedExercise(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("An exercise already exists at this position in the program day.", badRequest.Value);
        }


        #endregion

        #region POST

        [Fact]
        public async Task CreateTrainingProgram_ReturnsCreated()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.CreateTrainingProgram("user1", It.IsAny<TrainingProgramCreateDto>()))
                .ReturnsAsync(CreateProgram());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.CreateTrainingProgram(
                new TrainingProgramCreateDto { Name = "New Program" });

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task CreateProgramDay_ReturnsCreated()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.CreateProgramDay(It.IsAny<ProgramDayCreateDto>()))
                .ReturnsAsync(new ProgramDay { Id = 1 });

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.CreateProgramDay(
                new ProgramDayCreateDto { TrainingProgramId = 1, Name = "Day 1" });

            Assert.IsType<CreatedAtActionResult>(result);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task UpdateTrainingProgram_ReturnsNoContent()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.UpdateTrainingProgram(1, It.IsAny<TrainingProgramUpdateDto>()))
                .ReturnsAsync(CreateProgram());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.UpdateTrainingProgram(1, new TrainingProgramUpdateDto());

            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task DeleteTrainingProgram_ReturnsNoContent()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetDaysByProgramId(1))
                .ReturnsAsync(new List<ProgramDay>());

            repo.Setup(r => r.DeleteTrainingProgram(1))
                .ReturnsAsync(CreateProgram());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            var result = await controller.DeleteTrainingProgram(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTrainingProgram_DeletesAllDays()
        {
            var repo = new Mock<ITrainingProgramRepository>();

            repo.Setup(r => r.TrainingProgramExists("user1", 1))
                .ReturnsAsync(true);

            repo.Setup(r => r.GetDaysByProgramId(1))
                .ReturnsAsync(new List<ProgramDay>
                {
                    new ProgramDay { Id = 1 },
                    new ProgramDay { Id = 2 }
                });

            repo.Setup(r => r.DeleteTrainingProgram(1))
                .ReturnsAsync(CreateProgram());

            var controller = CreateController(repo, new Mock<IExerciseRepository>());

            await controller.DeleteTrainingProgram(1);

            repo.Verify(r => r.DeleteProgramDay(1), Times.Once);
            repo.Verify(r => r.DeleteProgramDay(2), Times.Once);
            repo.Verify(r => r.DeleteTrainingProgram(1), Times.Once);
        }
        
        #endregion
    }
}
