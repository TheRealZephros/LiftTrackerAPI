using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Exercise;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/exercise")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseSessionRepository _exerciseSessionRepository;
        private readonly ITrainingProgramRepository _trainingProgramRepository;
        private readonly IUserRepository _userRepository;
        public ExerciseController(IExerciseRepository exerciseRepository, IExerciseSessionRepository exerciseSessionRepository, ITrainingProgramRepository trainingProgramRepository, IUserRepository userRepository)
        {
            _exerciseRepository = exerciseRepository;
            _exerciseSessionRepository = exerciseSessionRepository;
            _trainingProgramRepository = trainingProgramRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExercises(string userId)
        {
            var exercises = await _exerciseRepository.GetAllAsync(userId);
            var exerciseDtos = exercises.Select(e => e.ToExerciseDto());
            return Ok(exerciseDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise.ToExerciseDto());
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateExercise([FromBody] ExerciseDto exercise)
        {
            if (exercise.UserId == null || !await _userRepository.UserExists(exercise.UserId))
            {
                return BadRequest("User does not exist.");
            }
            var createdExercise = await _exerciseRepository.AddAsync(exercise);
            if (createdExercise == null)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.Id }, createdExercise);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateExercise([FromRoute] int id, [FromBody] ExerciseDto exercise)
        {
            var updatedExercise = await _exerciseRepository.UpdateAsync(id, exercise);
            if (updatedExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteExercise([FromRoute] int id)
        {
            if (!await _exerciseRepository.ExerciseExists(id))
            {
                return NotFound();
            }
            // check if any exercise sessions exist with this exercise id
            if (await _exerciseSessionRepository.GetSessionsByExerciseId(id) != null)
                return BadRequest("Cannot delete exercise with existing exercise sessions.");
            else if (await _trainingProgramRepository.GetExercisesByExerciseId(id) != null)
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