using Api.Dtos.Exercise;
using Api.Extensions;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/exercises")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly ITrainingProgramRepository _trainingProgramRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(
            IExerciseRepository exerciseRepository,
            IExerciseSessionRepository exerciseSessionRepository,
            ITrainingProgramRepository trainingProgramRepository,
            UserManager<User> userManager,
            ILogger<ExerciseController> logger)
        {
            _exerciseRepository = exerciseRepository;
            _exerciseSessionRepository = exerciseSessionRepository;
            _trainingProgramRepository = trainingProgramRepository;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllExercises()
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is retrieving all exercises.", userId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to retrieve exercises.", userId);
                return BadRequest(ModelState);
            }
            var exercises = await _exerciseRepository.GetAllAsync(userId);
            if (exercises == null)
            {
                _logger.LogInformation("No exercises found for user {UserId}.", userId);
                return NotFound();
            }
            if (exercises.Any(e => e.UserId != userId && e.IsUsermade == true))
            {
                _logger.LogInformation("Unauthorized access attempt detected for user {UserId}.", userId);
                return NotFound();
            }
            var exerciseDtos = exercises.Select(e => e.ToExerciseDto());
            _logger.LogInformation("User {UserId} successfully retrieved {Count} exercises.", userId, exerciseDtos.Count());
            return Ok(exerciseDtos);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is retrieving exercise {ExerciseId}.", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to retrieve exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }
            var exercise = await _exerciseRepository.GetByIdAsync(userId, id);
            if (exercise == null)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found for user {UserId}.", id, userId);
                return NotFound();
            }
            if (exercise.UserId != userId && exercise.IsUsermade == true)
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId} for exercise {ExerciseId}.", userId, id);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} successfully retrieved exercise {ExerciseId}.", userId, id);
            return Ok(exercise.ToExerciseDto());
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateExercise([FromBody] ExerciseCreateDto exercise)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to create a new exercise.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create a new exercise.", userId);
                return BadRequest(ModelState);
            }

            var createdExercise = await _exerciseRepository.AddAsync(userId, exercise);
            if (createdExercise == null)
            {
                _logger.LogError("Failed to create exercise for user {UserId}.", userId);
                return BadRequest();
            }
            _logger.LogInformation("User {UserId} successfully created exercise {ExerciseId}.", userId, createdExercise.Id);
            return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.Id }, createdExercise);
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateExercise([FromRoute] int id, [FromBody] ExerciseUpdateDto exercise)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to update exercise {ExerciseId}.", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }
            var exists = await _exerciseRepository.ExerciseExists(userId, id);
            if (!exists)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found for user {UserId}.", id, userId);
                return NotFound();
            }

            var updatedExercise = await _exerciseRepository.UpdateAsync(id, exercise);
            if (updatedExercise == null)
            {
                _logger.LogError("Failed to update exercise {ExerciseId} for user {UserId}.", id, userId);
                return NotFound();
            }
            _logger.LogInformation("User {UserId} successfully updated exercise {ExerciseId}.", userId, id);
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExercise([FromRoute] int id)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to delete exercise {ExerciseId}.", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }

            var exists = await _exerciseRepository.ExerciseExists(userId, id);
            if (!exists)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found for user {UserId}.", id, userId);
                return NotFound();
            }
            // check if any exercise sessions exist with this exercise id
            var sessions = await _exerciseSessionRepository.GetSessionsByExerciseId(id);
            if (sessions != null && sessions.Any())
            {
                _logger.LogWarning("Cannot delete exercise {ExerciseId} for user {UserId} because it has existing exercise sessions.", id, userId);
                return BadRequest("Cannot delete exercise with existing exercise sessions.");
            }
            // check if any programmed exercises exist with this exercise id
            var programmedExercises = await _trainingProgramRepository.GetExercisesByExerciseId(id);
            if (programmedExercises != null && programmedExercises.Any())
            {
                _logger.LogWarning("Cannot delete exercise {ExerciseId} for user {UserId} because it has existing programmed exercises.", id, userId);
                return BadRequest("Cannot delete exercise with existing programmed exercises.");
            }
            var deletedExercise = await _exerciseRepository.DeleteAsync(id);
            if (deletedExercise == null)
            {
                _logger.LogWarning("Failed to delete exercise {ExerciseId} for user {UserId}.", id, userId);
                return NotFound();
            }
            _logger.LogInformation("User {UserId} successfully deleted exercise {ExerciseId}.", userId, id);
            return NoContent();
        }
    }
}