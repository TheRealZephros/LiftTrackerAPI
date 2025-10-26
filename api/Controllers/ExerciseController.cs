using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Exercise;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/exercises")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly ITrainingProgramRepository _trainingProgramRepository;
        private readonly UserManager<User> _userManager;

        public ExerciseController(IExerciseRepository exerciseRepository, IExerciseSessionRepository exerciseSessionRepository, ITrainingProgramRepository trainingProgramRepository, UserManager<User> userManager)
        {
            _exerciseRepository = exerciseRepository;
            _exerciseSessionRepository = exerciseSessionRepository;
            _trainingProgramRepository = trainingProgramRepository;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllExercises()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var exercises = await _exerciseRepository.GetAllAsync(userId);
            var exerciseDtos = exercises.Select(e => e.ToExerciseDto());
            return Ok(exerciseDtos);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var exercise = await _exerciseRepository.GetByIdAsync(userId, id);
            if (exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise.ToExerciseDto());
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateExercise([FromBody] ExerciseCreateDto exercise)
        {
            var userId = User.GetId();

            var createdExercise = await _exerciseRepository.AddAsync(userId, exercise);
            if (createdExercise == null)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.Id }, createdExercise);
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateExercise([FromRoute] int id, [FromBody] ExerciseUpdateDto exercise)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var exists = await _exerciseRepository.ExerciseExists(userId, id);
            if (!exists)
            {
                return NotFound();
            }

            var updatedExercise = await _exerciseRepository.UpdateAsync(id, exercise);
            if (updatedExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExercise([FromRoute] int id)
        {
            var userId = User.GetId();
            var exists = await _exerciseRepository.ExerciseExists(userId, id);
            if (!exists)
            {
                return NotFound();
            }
            // check if any exercise sessions exist with this exercise id
            var sessions = await _exerciseSessionRepository.GetSessionsByExerciseId(id);
            if (sessions != null && sessions.Any())
                return BadRequest("Cannot delete exercise with existing exercise sessions.");
            // check if any programmed exercises exist with this exercise id
            var programmedExercises = await _trainingProgramRepository.GetExercisesByExerciseId(id);
            if (programmedExercises != null && programmedExercises.Any())
                return BadRequest("Cannot delete exercise with existing programmed exercises.");
            var deletedExercise = await _exerciseRepository.DeleteAsync(id);
            if (deletedExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}