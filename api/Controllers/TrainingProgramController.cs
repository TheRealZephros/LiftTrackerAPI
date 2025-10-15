using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.TrainingProgram;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/program")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly UserManager<User> _userManager;
        private readonly IExerciseRepository _exerciseRepository;

        public TrainingProgramController(IExerciseRepository exercise_repository, ITrainingProgramRepository program_repository, UserManager<User> userManager)
        {
            _exerciseRepository = exercise_repository;
            _programRepository = program_repository;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTrainingProgramsForUser()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var programs = await _programRepository.GetTrainingProgramsForUser(userId);
            if (programs == null || !programs.Any())
            {
                return NotFound();
            }
            return Ok(programs);
        }

        [HttpGet("{programId}")]
        [Authorize]
        public async Task<IActionResult> GetTrainingProgramById([FromRoute] int programId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var program = await _programRepository.GetTrainingProgramById(programId);
            if (program == null || program.UserId != userId)
            {
                return NotFound();
            }
            return Ok(program);
        }

        [HttpGet("program/{programId}/days")]
        [Authorize]
        public async Task<IActionResult> GetDaysByProgramId([FromRoute] int programId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var days = await _programRepository.GetDaysByProgramId(programId);
            if (days == null || !days.Any() || days.First().TrainingProgram.UserId != userId)
            {
                return NotFound();
            }
            return Ok(days);
        }

        [HttpGet("day/{dayId}")]
        [Authorize]
        public async Task<IActionResult> GetDayById([FromRoute] int dayId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var day = await _programRepository.GetDayById(dayId);
            if (day == null || day.TrainingProgram.UserId != userId)
            {
                return NotFound();
            }
            return Ok(day);
        }

        [HttpGet("day/{dayId}/exercises")]
        public async Task<IActionResult> GetExercisesByDay([FromRoute] int dayId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                return NotFound();
            }
            var exercises = await _programRepository.GetExercisesByDay(dayId);
            if (exercises == null || !exercises.Any())
            {
                return NotFound();
            }
            return Ok(exercises);
        }

        [HttpGet("exercise/{id}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                return NotFound();
            }
            var exercise = await _programRepository.GetExerciseById(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTrainingProgram([FromBody] TrainingProgramCreateDto programDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            var newProgram = await _programRepository.CreateTrainingProgram(userId, programDto);
            if (newProgram == null)
            {
                return BadRequest("Could not create training program.");
            }
            return CreatedAtAction(nameof(GetTrainingProgramById), new { userId = newProgram.UserId, programId = newProgram.Id }, newProgram);
        }

        [HttpPost("day/create")]
        [Authorize]
        public async Task<IActionResult> CreateProgramDay([FromBody] ProgramDayCreateDto programDayDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.TrainingProgramExists(userId, programDayDto.TrainingProgramId))
            {
                return BadRequest("Training program does not exist.");
            }
            var day = await _programRepository.CreateProgramDay(programDayDto);
            if (day == null)
            {
                return NotFound();
            }
            return CreatedAtAction(nameof(GetDayById), new { id = day.Id }, day);
        }

        [HttpPost("exercise/create")]
        [Authorize]
        public async Task<IActionResult> CreateProgrammedExercise([FromBody] ProgrammedExerciseCreateDto exerciseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();

            if (!await _programRepository.ProgramDayExists(userId, exerciseDto.ProgramDayId))
                return BadRequest("Program day does not exist.");
            if (!await _exerciseRepository.ExerciseExists(userId, exerciseDto.ExerciseId))
                return BadRequest("Exercise does not exist.");
            if (await _programRepository.ProgrammedExercisePositionExists(userId, exerciseDto.ProgramDayId, exerciseDto.Position))
                return BadRequest("An exercise already exists at this position in the program day.");
            var newExercise = await _programRepository.CreateProgrammedExercise(exerciseDto);
            if (newExercise == null)
            {
                return NotFound();
            }
            return CreatedAtAction(nameof(GetExerciseById), new { id = newExercise.Id }, newExercise);
        }

        [HttpPut("update/{programId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTrainingProgram([FromRoute] int programId, [FromBody] TrainingProgramUpdateDto programDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.TrainingProgramExists(userId, programId))
            {
                return NotFound();
            }
            var updated_program = await _programRepository.UpdateTrainingProgram(programId, programDto);
            if (updated_program == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("update/day/{dayId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProgramDay([FromRoute] int dayId, [FromBody] ProgramDayUpdateDto dayDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                return NotFound();
            }
            var existingDay = await _programRepository.UpdateProgramDay(dayId, dayDto);
            if (existingDay == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("update/exercise/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProgrammedExercise([FromRoute] int id, [FromBody] ProgrammedExerciseUpdateDto exerciseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                return NotFound();
            }
            var existingExercise = await _programRepository.UpdateProgrammedExercise(id, exerciseDto);
            if (existingExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteTrainingProgram(int programId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.TrainingProgramExists(userId, programId))
            {
                return NotFound();
            }
            var days = await _programRepository.GetDaysByProgramId(programId);
            if (days != null)
            {
                foreach (var day in days)
                {
                    if (day == null) continue;
                    await _programRepository.DeleteProgramDay(day.Id);
                }
            }
            await _programRepository.DeleteTrainingProgram(programId);
            return NoContent();
        }

        [HttpDelete("delete/day")]
        [Authorize]
        public async Task<IActionResult> DeleteProgramDay(int dayId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                return NotFound();
            }
            await _programRepository.DeleteProgramDay(dayId);
            return NoContent();
        }

        [HttpDelete("delete/exercise/")]
        [Authorize]
        public async Task<IActionResult> DeleteProgrammedExercise(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                return NotFound();
            }
            var existingExercise = await _programRepository.DeleteProgrammedExercise(id);
            if (existingExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}