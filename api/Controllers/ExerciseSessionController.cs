using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.ExerciseSession;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/session")]
    [ApiController]
    public class ExerciseSessionController : ControllerBase
    {
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUserRepository _userRepository;
        public ExerciseSessionController(IExerciseSessionRepository exerciseSessionRepository, IExerciseRepository exerciseRepository, IUserRepository userRepository)
        {
            _exerciseSessionRepository = exerciseSessionRepository;
            _exerciseRepository = exerciseRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetExerciseSessionsForUser(string userId)
        {
            var sessions = await _exerciseSessionRepository.GetAllAsync(userId);
            return Ok(sessions);
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetExerciseSessionById([FromRoute] int sessionId)
        {
            var session = await _exerciseSessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session.ToExerciseSessionDto());
        }

        [HttpGet("{sessionId}/set")]
        public async Task<IActionResult> GetExerciseSetsForSession([FromRoute] int sessionId)
        {
            var sets = await _exerciseSessionRepository.GetSetsBySessionIdAsync(sessionId);
            if (sets == null || !sets.Any())
            {
                return NotFound();
            }

            return Ok(sets.Select(s => s.ToExerciseSetDto()).ToList());
        }

        [HttpGet("set/{setId}")]
        public async Task<IActionResult> GetExerciseSetById([FromRoute] int setId)
        {
            var set = await _exerciseSessionRepository.GetSetByIdAsync(setId);
            if (set == null)
            {
                return NotFound();
            }
            return Ok(set.ToExerciseSetDto());
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateExerciseSession([FromBody] ExerciseSessionCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid session data.");
            if (!await _userRepository.UserExists(dto.UserId))
                return BadRequest("User does not exist.");
            if (!await _exerciseRepository.ExerciseExists(dto.ExerciseId))
                return BadRequest("Exercise does not exist.");
            var newSession = await _exerciseSessionRepository.AddAsync(dto);
            if (newSession == null)
                return StatusCode(500, "A problem happened while handling your request.");
            return CreatedAtAction(nameof(GetExerciseSessionById), new { sessionId = newSession.Id }, newSession.ToExerciseSessionDto());
        }

        [HttpPost("set/create")]
        public async Task<IActionResult> CreateExerciseSet([FromBody] ExerciseSetCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid set data.");
            }
            if (!await _exerciseSessionRepository.ExerciseSessionExists(dto.ExerciseSessionId))
            {
                return BadRequest("Exercise session does not exist.");
            }
            if (!await _exerciseRepository.ExerciseExists(dto.ExerciseId))
            {
                return BadRequest("Exercise does not exist.");
            }
            var newSet = await _exerciseSessionRepository.AddSetAsync(dto);
            if (newSet == null)
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
            return CreatedAtAction(nameof(GetExerciseSetById), new { setId = newSet.Id }, newSet.ToExerciseSetDto());
        }

        [HttpPut("update/{sessionId}")]
        public async Task<IActionResult> UpdateExerciseSession([FromRoute] int sessionId, [FromBody] ExerciseSessionDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid session data.");
            }

            var updatedSession = await _exerciseSessionRepository.UpdateAsync(sessionId, dto);
            if (updatedSession == null)
            {
                return NotFound();
            }

            return Ok(updatedSession.ToExerciseSessionDto());
        }

        [HttpPut("set/update/{setId}")]
        public async Task<IActionResult> UpdateExerciseSet([FromRoute] int setId, [FromBody] ExerciseSetDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid set data.");
            }

            var updatedSet = await _exerciseSessionRepository.UpdateSetAsync(setId, dto);
            if (updatedSet == null)
            {
                return NotFound();
            }

            return Ok(updatedSet.ToExerciseSetDto());
        }

        [HttpDelete("delete/{sessionId}")]
        public async Task<IActionResult> DeleteExerciseSession([FromRoute] int sessionId)
        {
            foreach (var set in await _exerciseSessionRepository.GetSetsBySessionIdAsync(sessionId))
            {
                await _exerciseSessionRepository.DeleteSetAsync(set.Id);
            }
            var deletedSession = await _exerciseSessionRepository.DeleteAsync(sessionId);
            if (deletedSession == null)
            {
                return NotFound();
            }

            return NoContent();
        }
        [HttpDelete("set/delete/{setId}")]
        public async Task<IActionResult> DeleteExerciseSet([FromRoute] int setId)
        {
            var deletedSet = await _exerciseSessionRepository.DeleteSetAsync(setId);
            if (deletedSet == null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
