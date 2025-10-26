using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.ExerciseSession;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class ExerciseSessionController : ControllerBase
    {
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly UserManager<User> _userManager;

        public ExerciseSessionController(IExerciseSessionRepository exerciseSessionRepository, IExerciseRepository exerciseRepository, UserManager<User> userManager)
        {
            _exerciseSessionRepository = exerciseSessionRepository;
            _exerciseRepository = exerciseRepository;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetExerciseSessionsForUser()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var sessions = await _exerciseSessionRepository.GetAllAsync(userId);
            return Ok(sessions.Select(s => s.ToExerciseSessionDto()).ToList());
        }

        [HttpGet("{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSessionById([FromRoute] int sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var session = await _exerciseSessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound();
            }
            return Ok(session.ToExerciseSessionDto());
        }

        [HttpGet("{sessionId}/sets")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSetsForSession([FromRoute] int sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var sessionExists = await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId);
            if (!sessionExists)
            {
                return NotFound();
            }
            var sets = await _exerciseSessionRepository.GetSetsBySessionIdAsync(sessionId);
            if (sets == null)
            {
                return NotFound();
            }
            return Ok(sets.Select(s => s.ToExerciseSetDto()).ToList());
        }

        [HttpGet("sets/{setId}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseSetById([FromRoute] int setId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var set = await _exerciseSessionRepository.GetSetByIdAsync(setId);
            if (set == null || set.ExerciseSession.UserId != userId)
            {
                return NotFound();
            }
            return Ok(set.ToExerciseSetDto());
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateExerciseSession([FromBody] ExerciseSessionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var exercise = await _exerciseRepository.GetByIdAsync(userId, dto.ExerciseId);
            if (exercise == null || exercise.UserId != userId)
                return BadRequest("Exercise does not exist.");

            var newSession = await _exerciseSessionRepository.AddAsync(userId, dto);
            if (newSession == null)
                return StatusCode(500, "A problem happened while handling your request.");
            return CreatedAtAction(nameof(GetExerciseSessionById), new { sessionId = newSession.Id }, newSession.ToExerciseSessionDto());
        }

        [HttpPost("sets/create")]
        [Authorize]
        public async Task<IActionResult> CreateExerciseSet([FromBody] ExerciseSetCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();

            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, dto.ExerciseSessionId))
                return BadRequest("Exercise session does not exist.");
            var session = await _exerciseSessionRepository.GetByIdAsync(dto.ExerciseSessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound();
            }

            var newSet = await _exerciseSessionRepository.AddSetAsync(session.ExerciseId,dto);
            if (newSet == null)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
            return CreatedAtAction(nameof(GetExerciseSetById), new { setId = newSet.Id }, newSet.ToExerciseSetDto());
        }

        [HttpPut("update/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateExerciseSession([FromRoute] int sessionId, [FromBody] ExerciseSessionUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId))
            {
                return NotFound();
            }

            var updatedSession = await _exerciseSessionRepository.UpdateAsync(sessionId, dto);
            if (updatedSession == null)
            {
                return NotFound();
            }

            return Ok(updatedSession.ToExerciseSessionDto());
        }

        [HttpPut("sets/update/{setId}")]
        [Authorize]
        public async Task<IActionResult> UpdateExerciseSet([FromRoute] int setId, [FromBody] ExerciseSetUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _exerciseSessionRepository.ExerciseSetExists(userId, setId))
            {
                return NotFound();
            }

            var updatedSet = await _exerciseSessionRepository.UpdateSetAsync(setId, dto);
            if (updatedSet == null)
            {
                return NotFound();
            }

            return Ok(updatedSet.ToExerciseSetDto());
        }

        [HttpDelete("delete/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteExerciseSession([FromRoute] int sessionId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _exerciseSessionRepository.ExerciseSessionExists(userId, sessionId))
            {
                return NotFound();
            }
            
            var deletedSession = await _exerciseSessionRepository.DeleteAsync(sessionId);
            if (deletedSession == null)
            {
                return NotFound();
            }

            return NoContent();
        }
        [HttpDelete("sets/delete/{setId}")]
        [Authorize]
        public async Task<IActionResult> DeleteExerciseSet([FromRoute] int setId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _exerciseSessionRepository.ExerciseSetExists(userId, setId))
            {
                return NotFound();
            }
            var deletedSet = await _exerciseSessionRepository.DeleteSetAsync(setId);
            if (deletedSet == null)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
