using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.TrainingProgram;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/program")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExerciseRepository _exerciseRepository;
        public TrainingProgramController(ITrainingProgramRepository program_repository, IUserRepository user_repository, IExerciseRepository exercise_repository)
        {
            _programRepository = program_repository;
            _userRepository = user_repository;
            _exerciseRepository = exercise_repository;
        }
        // TODO authenticate
        // TODO redo this to use authentication to determine user id
        [HttpGet]
        public async Task<IActionResult> GetTrainingProgramsForUser(int userId)
        {
            var programs = await _programRepository.GetTrainingProgramsForUser(userId);
            return Ok(programs);
        }

        // TODO authenticate
        [HttpGet("{programId}")]
        public async Task<IActionResult> GetTrainingProgramById([FromRoute] int programId)
        {
            var program = await _programRepository.GetTrainingProgramById(programId);
            if (program == null)
            {
                return NotFound();
            }
            return Ok(program);
        }

        [HttpGet("program/{programId}/days")]
        public async Task<IActionResult> GetDaysByProgramId([FromRoute] int programId)
        {
            var days = await _programRepository.GetDaysByProgramId(programId);
            if (days == null || !days.Any())
            {
                return NotFound();
            }
            return Ok(days);
        }

        [HttpGet("day/{dayId}")]
        public async Task<IActionResult> GetDayById([FromRoute] int dayId)
        {
            var day = await _programRepository.GetDayById(dayId);
            if (day == null)
            {
                return NotFound();
            }
            return Ok(day);
        }

        [HttpGet("day/{dayId}/exercises")]
        public async Task<IActionResult> GetExercisesByDay([FromRoute] int dayId)
        {
            var exercises = await _programRepository.GetExercisesByDay(dayId);
            if (exercises == null || !exercises.Any())
            {
                return NotFound();
            }
            return Ok(exercises);
        }

        [HttpGet("exercise/{id}")]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            var exercise = await _programRepository.GetExerciseById(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTrainingProgram([FromBody] TrainingProgramCreateDto programDto)
        {
            if (!await _userRepository.UserExists(programDto.UserId))
            {
                return BadRequest("User does not exist.");
            }
            var newProgram = await _programRepository.CreateTrainingProgram(programDto);
            if (newProgram == null)
            {
                return BadRequest("Could not create training program.");
            }
            return CreatedAtAction(nameof(GetTrainingProgramById), new { userId = newProgram.UserId, programId = newProgram.Id }, newProgram);
        }
        [HttpPost("day/create")]
        public async Task<IActionResult> CreateProgramDay([FromBody] ProgramDayCreateDto programDayDto)
        {
            if (!await _programRepository.TrainingProgramExists(programDayDto.TrainingProgramId))
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
        public async Task<IActionResult> CreateProgrammedExercise([FromBody] ProgrammedExerciseCreateDto exerciseDto)
        {
            if (!await _userRepository.UserExists(exerciseDto.UserId))
                return BadRequest("User does not exist.");
            if (!await _programRepository.ProgramDayExists(exerciseDto.ProgramDayId))
                return BadRequest("Program day does not exist.");
            if (!await _exerciseRepository.ExerciseExists(exerciseDto.ExerciseId))
                return BadRequest("Exercise does not exist.");
            if (await _programRepository.ProgrammedExercisePositionExists(exerciseDto.ProgramDayId, exerciseDto.Position))
                return BadRequest("An exercise already exists at this position in the program day.");
            var newExercise = await _programRepository.CreateProgrammedExercise(exerciseDto);
            if (newExercise == null)
            {
                return NotFound();
            }
            return CreatedAtAction(nameof(GetExerciseById), new { id = newExercise.Id }, newExercise);
        }

        // TODO authenticate
        // TODO redo this to use authentication to determine user id
        [HttpPut("update/{programId}")]
        public async Task<IActionResult> UpdateTrainingProgram([FromRoute] int programId, [FromBody] TrainingProgramUpdateDto programDto)
        {
            var existing_program = await _programRepository.UpdateTrainingProgram(programId, programDto);
            if (existing_program == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("update/day/{dayId}")]
        public async Task<IActionResult> UpdateProgramDay([FromRoute] int dayId, [FromBody] ProgramDayUpdateDto dayDto)
        {
            var existingDay = await _programRepository.UpdateProgramDay(dayId, dayDto);
            if (existingDay == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("update/exercise/{id}")]
        public async Task<IActionResult> UpdateProgrammedExercise([FromRoute] int id, [FromBody] ProgrammedExerciseUpdateDto exerciseDto)
        {
            var existingExercise = await _programRepository.UpdateProgrammedExercise(id, exerciseDto);
            if (existingExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }


        // TODO authenticate
        // TODO redo this to use authentication to determine user id
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTrainingProgram(int programId)
        {
            if (!await _programRepository.TrainingProgramExists(programId))
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

        // TODO authenticate
        // TODO redo this to use authentication to determine user id
        [HttpDelete("delete/day")]
        public async Task<IActionResult> DeleteProgramDay(int dayId)
        {
            if (!await _programRepository.ProgramDayExists(dayId))
            {
                return NotFound();
            }
            var exercises = await _programRepository.GetExercisesByDay(dayId);
            if (exercises != null)
            {
                foreach (var exercise in exercises)
                {
                    if (exercise == null) continue;
                    await _programRepository.DeleteProgrammedExercise(exercise.Id);
                }
            }
            await _programRepository.DeleteProgramDay(dayId);
            return NoContent();
        }
        
        // TODO authenticate
        [HttpDelete("delete/exercise/")]
        public async Task<IActionResult> DeleteProgrammedExercise(int id)
        {
            var existingExercise = await _programRepository.DeleteProgrammedExercise(id);
            if (existingExercise == null)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}