using Api.Dtos.ExerciseSession;
using Api.Extensions;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class ExerciseSessionController : ControllerBase
    {
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExerciseSessionController> _logger;

        public ExerciseSessionController(
            IExerciseSessionRepository exerciseSessionRepository,
            IExerciseRepository exerciseRepository,
            UserManager<User> userManager,
            ILogger<ExerciseSessionController> logger
        )
        {
            _exerciseSessionRepository = exerciseSessionRepository;
            _exerciseRepository = exerciseRepository;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetExerciseSessionsForUser()
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested all exercise sessions.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested all exercise sessions.", userId);
                return BadRequest(ModelState);
            }
            var sessions = await _exerciseSessionRepository.GetAllAsync(userId);
            _logger.LogInformation("Successfully retrieved {SessionCount} exercise sessions for user {UserId}.", sessions.Count, userId);
            return Ok(sessions.Select(s => s.ToExerciseSessionDto()).ToList());
        }

        [HttpGet("{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSessionById([FromRoute] int sessionId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercise session {SessionId}.", userId, sessionId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercise session {SessionId}.", userId, sessionId);
                return BadRequest(ModelState);
            }
            var session = await _exerciseSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Exercise session {SessionId} not found for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            if (session.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId} for exercise session {SessionId}.", userId, sessionId);
                return NotFound();
            }
            _logger.LogInformation("Successfully retrieved exercise session {SessionId} for user {UserId}.", sessionId, userId);
            return Ok(session.ToExerciseSessionDto());
        }

        [HttpGet("exercise/{exerciseId}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSessionByExerciseId([FromRoute] int exerciseId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercise sessions for exercise {ExerciseId}.", userId, exerciseId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercise sessions for exercise {ExerciseId}.", userId, exerciseId);
                return BadRequest(ModelState);
            }
            var sessions = await _exerciseSessionRepository.GetSessionsByExerciseId(exerciseId);
            if (sessions == null)
            {
                _logger.LogWarning("No exercise sessions found for exercise {ExerciseId} for user {UserId}.", exerciseId, userId);
                return NotFound();
            }
            
            if (sessions.Any(s => s.UserId != userId))
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId} for exercise sessions of exercise {ExerciseId}.", userId, exerciseId);
                return NotFound();
            }
            _logger.LogInformation("Successfully retrieved {SessionCount} exercise sessions for exercise {ExerciseId} for user {UserId}.", sessions.Count, exerciseId, userId);
            return Ok(sessions.Select(s => s.ToExerciseSessionDto()).ToList());
        }

        [HttpGet("{sessionId}/sets")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSetsForSession([FromRoute] int sessionId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercise sets for session {SessionId}.", userId, sessionId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercise sets for session {SessionId}.", userId, sessionId);
                return BadRequest(ModelState);
            }
            var sessionExists = await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId);
            if (!sessionExists)
            {
                _logger.LogWarning("Exercise session {SessionId} not found for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            var sets = await _exerciseSessionRepository.GetSetsBySessionIdAsync(sessionId);
            if (sets == null)
            {
                _logger.LogWarning("No exercise sets found for session {SessionId} for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            _logger.LogInformation("Successfully retrieved {SetCount} exercise sets for session {SessionId} for user {UserId}.", sets.Count, sessionId, userId);
            return Ok(sets.Select(s => s.ToExerciseSetDto()).ToList());
        }

        [HttpGet("sets/{setId}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSetById([FromRoute] int setId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercise set {SetId}.", userId, setId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercise set {SetId}.", userId, setId);
                return BadRequest(ModelState);
            }
            var set = await _exerciseSessionRepository.GetSetByIdAsync(setId);
            if (set == null)
            {
                _logger.LogWarning("Exercise set {SetId} not found for user {UserId}.", setId, userId);
                return NotFound();
            }
            if (set.ExerciseSession.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt by user {UserId} for exercise set {SetId}.", userId, setId);
                return NotFound();
            }
            _logger.LogInformation("Successfully retrieved exercise set {SetId} for user {UserId}.", setId, userId);
            return Ok(set.ToExerciseSetDto());
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateExerciseSession([FromBody] ExerciseSessionCreateDto dto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to create a new exercise session.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create an exercise session.", userId);
                return BadRequest(ModelState);
            }
            var exercise = await _exerciseRepository.GetByIdAsync(userId, dto.ExerciseId);
            if (exercise == null)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found for user {UserId}.", dto.ExerciseId, userId);
                return BadRequest("Exercise does not exist.");
            }
            if (exercise.UserId != userId)
            {
                _logger.LogWarning("Unauthorized exercise access attempt by user {UserId} for exercise {ExerciseId}.", userId, dto.ExerciseId);
                return BadRequest("Exercise does not exist.");
            }
            var newSession = await _exerciseSessionRepository.AddAsync(userId, dto);
            if (newSession == null)
            {
                _logger.LogError("Failed to create a new exercise session for user {UserId}.", userId);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            _logger.LogInformation("User {UserId} successfully created a new exercise session with ID {SessionId}.", userId, newSession.Id);
            return CreatedAtAction(nameof(GetExerciseSessionById), new { sessionId = newSession.Id }, newSession.ToExerciseSessionDto());
        }

        [HttpPost("sets/create")]
        [Authorize]
        public async Task<IActionResult> CreateExerciseSet([FromBody] ExerciseSetCreateDto dto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to create a new exercise set.", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create an exercise set.", userId);
                return BadRequest(ModelState);
            }
            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, dto.ExerciseSessionId))
            {
                _logger.LogWarning("Exercise session {ExerciseSessionId} not found for user {UserId}.", dto.ExerciseSessionId, userId);
                return BadRequest("Exercise session does not exist.");
            }
            var session = await _exerciseSessionRepository.GetByIdAsync(dto.ExerciseSessionId);
            if (session == null)
            {
                return NotFound();
            }
            if(session.UserId != userId)
            {
                _logger.LogWarning("Unauthorized exercise session access attempt by user {UserId} for session {ExerciseSessionId}.", userId, dto.ExerciseSessionId);
                return BadRequest("Exercise session does not exist.");
            }

            var newSet = await _exerciseSessionRepository.AddSetAsync(session.ExerciseId,dto);
            if (newSet == null)
            {
                _logger.LogError("Failed to create a new exercise set for user {UserId}.", userId);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            _logger.LogInformation("User {UserId} successfully created a new exercise set with ID {SetId}.", userId, newSet.Id);
            return CreatedAtAction(nameof(GetExerciseSetById), new { setId = newSet.Id }, newSet.ToExerciseSetDto());
        }

        [HttpPut("update/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateExerciseSession([FromRoute] int sessionId, [FromBody] ExerciseSessionUpdateDto dto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to update exercise session {SessionId}.", userId, sessionId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update exercise session {SessionId}.", userId, sessionId);
                return BadRequest(ModelState);
            }
            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId))
            {
                _logger.LogWarning("Exercise session {SessionId} not found for user {UserId}.", sessionId, userId);
                return NotFound();
            }

            var updatedSession = await _exerciseSessionRepository.UpdateAsync(sessionId, dto);
            if (updatedSession == null)
            {
                _logger.LogWarning("Failed to update exercise session {SessionId} for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            _logger.LogInformation("User {UserId} successfully updated exercise session {SessionId}.", userId, sessionId);
            return Ok(updatedSession.ToExerciseSessionDto());
        }

        [HttpPut("sets/update/{setId}")]
        [Authorize]
        public async Task<IActionResult> UpdateExerciseSet([FromRoute] int setId, [FromBody] ExerciseSetUpdateDto dto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to update exercise set {SetId}.", userId, setId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update exercise set {SetId}.", userId, setId);
                return BadRequest(ModelState);
            }
            if (!await _exerciseSessionRepository.ExerciseSetExists(userId, setId))
            {
                _logger.LogWarning("Exercise set {SetId} not found for user {UserId}.", setId, userId);
                return NotFound();
            }

            var updatedSet = await _exerciseSessionRepository.UpdateSetAsync(setId, dto);
            if (updatedSet == null)
            {
                _logger.LogWarning("Failed to update exercise set {SetId} for user {UserId}.", setId, userId);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} successfully updated exercise set {SetId}.", userId, setId);
            return Ok(updatedSet.ToExerciseSetDto());
        }

        [HttpDelete("delete/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteExerciseSession([FromRoute] int sessionId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to delete exercise session {SessionId}.", userId, sessionId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete exercise session {SessionId}.", userId, sessionId);
                return BadRequest(ModelState);
            }
            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId))
            {
                _logger.LogWarning("Exercise session {SessionId} not found for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            
            var deletedSession = await _exerciseSessionRepository.DeleteAsync(sessionId);
            if (deletedSession == null)
            {
                _logger.LogWarning("Failed to delete exercise session {SessionId} for user {UserId}.", sessionId, userId);
                return NotFound();
            }
            _logger.LogInformation("User {UserId} successfully deleted exercise session {SessionId}.", userId, sessionId);
            return NoContent();
        }
        [HttpDelete("sets/delete/{setId}")]
        [Authorize]
        public async Task<IActionResult> DeleteExerciseSet([FromRoute] int setId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is attempting to delete exercise set {SetId}.", userId, setId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete exercise set {SetId}.", userId, setId);
                return BadRequest(ModelState);
            }
            if (!await _exerciseSessionRepository.ExerciseSetExists(userId, setId))
            {
                _logger.LogWarning("Exercise set {SetId} not found for user {UserId}.", setId, userId);
                return NotFound();
            }
            var deletedSet = await _exerciseSessionRepository.DeleteSetAsync(setId);
            if (deletedSet == null)
            {
                _logger.LogWarning("Failed to delete exercise set {SetId} for user {UserId}.", setId, userId);
                return NotFound();
            }
            _logger.LogInformation("User {UserId} successfully deleted exercise set {SetId}.", userId, setId);
            return NoContent();
        }
    }
}
