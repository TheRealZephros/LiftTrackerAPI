using Api.Dtos.TrainingProgram;
using Api.Extensions;
using Api.Interfaces;
using Api.Mappers;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/programs")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private readonly ITrainingProgramRepository _programRepository;
        private readonly UserManager<User> _userManager;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<TrainingProgramController> _logger;

        public TrainingProgramController(
            IExerciseRepository exercise_repository,
            ITrainingProgramRepository program_repository,
            UserManager<User> userManager,
            ILogger<TrainingProgramController> logger)
        {
            _exerciseRepository = exercise_repository;
            _programRepository = program_repository;
            _userManager = userManager;
            _logger = logger;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTrainingProgramsForUser()
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested all training programs.", userId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested all training programs.", userId);
                return BadRequest(ModelState);
            }
            
            var programs = await _programRepository.GetTrainingProgramsForUser(userId);
            if (programs == null)
            {
                _logger.LogWarning("Training programs are null. Requested by user {UserId}.", userId);
                return NotFound();
            }
            var programDtos = new List<TrainingProgramGetAllDto>();
            foreach ( var p in programs)
            {
                programDtos.Add(new TrainingProgramGetAllDto
                {
                    Id = p.Id,
                    Name = p.Name
                });
            }
            return Ok(programDtos);
        }

        [HttpGet("{programId}")]
        [Authorize]
        public async Task<IActionResult> GetTrainingProgramById([FromRoute] int programId)
        {
            var userId = User.GetId();
             _logger.LogDebug("User {UserId} requested training program {ProgramId}.", userId, programId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested training program {ProgramId}.", userId, programId);
                return BadRequest(ModelState);
            }
            
            var program = await _programRepository.GetTrainingProgramById(programId);
            if (program == null || program.UserId != userId)
            {
                _logger.LogWarning("Training program {ProgramId} is null or does not belong to user {UserId}.", programId, userId);
                return NotFound();
            }
            var programDto = new TrainingProgramGetByIdDto
            {
                Id = program.Id,
                UserId = program.UserId,
                Name = program.Name,
                Description = program.Description,
                IsWeekDaySynced = program.IsWeekDaySynced,
                CreatedAt = program.CreatedAt,
                Days = program.Days?.Select(d => d.ToProgramDayDto()).ToList() ?? new List<ProgramDayDto>()
            };
            _logger.LogInformation("Training program {ProgramId} retrieved successfully for user {UserId}.", programId, userId);
            return Ok(programDto);
        }

        [HttpGet("program/{programId}/days")]
        [Authorize]
        public async Task<IActionResult> GetDaysByProgramId([FromRoute] int programId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested days for training program {ProgramId}.", userId, programId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested days for training program {ProgramId}.", userId, programId);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.TrainingProgramExists(userId, programId))
            {
                _logger.LogWarning("User {UserId} attempted to access non-existing program {ProgramId}.", userId, programId);
                return NotFound();
            }
            var days = await _programRepository.GetDaysByProgramId(programId);

            if (days == null)
            {
                _logger.LogWarning("Days are null for training program {ProgramId}. Requested by user {UserId}.", programId, userId);
                return NotFound();
            }
            _logger.LogInformation("Days for training program {ProgramId} retrieved successfully for user {UserId}.", programId, userId);
            return Ok(days.Select(d => d.ToProgramDayDto()).ToList());
        }

        [HttpGet("days/{dayId}")]
        [Authorize]
        public async Task<IActionResult> GetDayById([FromRoute] int dayId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested day {DayId}.", userId, dayId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested day {DayId}.", userId, dayId);
                return BadRequest(ModelState);
            }
            
            var day = await _programRepository.GetDayById(dayId);
            if (day == null)
            {
                _logger.LogWarning("Day {DayId} is null. Requested by user {UserId}.", dayId, userId);
                return NotFound();
            }
            if (!await _programRepository.TrainingProgramExists(userId, day.TrainingProgramId))
            {
                _logger.LogWarning("User {UserId} attempted to access day {DayId} belonging to a non-existing or unauthorized training program {ProgramId}.", userId, dayId, day.TrainingProgramId);
                return NotFound();
            }

            return Ok(day.ToProgramDayDto());
        }

        [HttpGet("days/{dayId}/exercises")]
        public async Task<IActionResult> GetExercisesByDay([FromRoute] int dayId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercises for day {DayId}.", userId, dayId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercises for day {DayId}.", userId, dayId);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                _logger.LogWarning("User {UserId} attempted to access non-existing or unauthorized day {DayId}.", userId, dayId);
                return NotFound();
            }
            var exercises = await _programRepository.GetExercisesByDay(dayId);
            if (exercises == null)
            {
                _logger.LogWarning("Exercises are null for day {DayId} requested by user {UserId}.", dayId, userId);
                return NotFound();
            }
            _logger.LogInformation("Exercises for day {DayId} retrieved successfully for user {UserId}.", dayId, userId);
            return Ok(exercises.Select(e => e.ToProgrammedExerciseDto()).ToList());
        }

        [HttpGet("exercises/{id}")]
        [Authorize]
        public async Task<IActionResult> GetExerciseById([FromRoute] int id)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} requested exercise {ExerciseId}.", userId, id);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} requested exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                _logger.LogWarning("User {UserId} attempted to access non-existing or unauthorized exercise {ExerciseId}.", userId, id);
                return NotFound();
            }
            var exercise = await _programRepository.GetExerciseById(id);
            if (exercise == null)
            {
                _logger.LogWarning("Exercise {ExerciseId} is null. Requested by user {UserId}.", id, userId);
                return NotFound();
            }
            _logger.LogInformation("Exercise {ExerciseId} retrieved successfully for user {UserId}.", id, userId);
            return Ok(exercise.ToProgrammedExerciseDto());
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTrainingProgram([FromBody] TrainingProgramCreateDto programDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is creating a new training program.", userId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create a training program.", userId);
                return BadRequest(ModelState);
            }
            
            var newProgram = await _programRepository.CreateTrainingProgram(userId, programDto);
            if (newProgram == null)
            {
                _logger.LogError("Failed to create training program for user {UserId}.", userId);
                return BadRequest("Could not create training program.");
            }
            _logger.LogInformation("Training program {ProgramId} created successfully for user {UserId}.", newProgram.Id, userId);
            return CreatedAtAction(nameof(GetTrainingProgramById), new { userId = newProgram.UserId, programId = newProgram.Id }, newProgram);
        }

        [HttpPost("days/create")]
        [Authorize]
        public async Task<IActionResult> CreateProgramDay([FromBody] ProgramDayCreateDto programDayDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is creating a new program day for training program {ProgramId}.", userId, programDayDto.TrainingProgramId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create a program day.", userId);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.TrainingProgramExists(userId, programDayDto.TrainingProgramId))
            {
                _logger.LogWarning("User {UserId} attempted to create a program day for non-existing or unauthorized training program {ProgramId}.", userId, programDayDto.TrainingProgramId);
                return BadRequest("Training program does not exist.");
            }
            var day = await _programRepository.CreateProgramDay(programDayDto);
            if (day == null)
            {
                _logger.LogError("Failed to create program day for user {UserId} in training program {ProgramId}.", userId, programDayDto.TrainingProgramId);
                return NotFound();
            }
            _logger.LogInformation("Program day {DayId} created successfully for user {UserId}.", day.Id, userId);
            return CreatedAtAction(nameof(GetDayById), new { dayId = day.Id }, day);
        }

        [HttpPost("exercises/create")]
        [Authorize]
        public async Task<IActionResult> CreateProgrammedExercise([FromBody] ProgrammedExerciseCreateDto exerciseDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is creating a new programmed exercise for program day {ProgramDayId}.", userId, exerciseDto.ProgramDayId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to create a programmed exercise.", userId);
                return BadRequest(ModelState);
            }
            if (!await _programRepository.ProgramDayExists(userId, exerciseDto.ProgramDayId))
            {
                _logger.LogWarning("User {UserId} attempted to create a programmed exercise for non-existing or unauthorized program day {ProgramDayId}.", userId, exerciseDto.ProgramDayId);
                return BadRequest("Program day does not exist.");
            }
            if (!await _exerciseRepository.ExerciseExists(userId, exerciseDto.ExerciseId))
            {
                _logger.LogWarning("User {UserId} attempted to create a programmed exercise with non-existing or unauthorized exercise {ExerciseId}.", userId, exerciseDto.ExerciseId);
                return BadRequest("Exercise does not exist.");
            }
            if (await _programRepository.ProgrammedExercisePositionExists(userId, exerciseDto.ProgramDayId, exerciseDto.Position))
            {
                _logger.LogWarning("User {UserId} attempted to create a programmed exercise at an existing position {Position} in program day {ProgramDayId}.", userId, exerciseDto.Position, exerciseDto.ProgramDayId);
                return BadRequest("An exercise already exists at this position in the program day.");
            }
            var newExercise = await _programRepository.CreateProgrammedExercise(exerciseDto);
            if (newExercise == null)
            {
                _logger.LogError("Failed to create programmed exercise for user {UserId} in program day {ProgramDayId}.", userId, exerciseDto.ProgramDayId);
                return NotFound();
            }
            _logger.LogInformation("Programmed exercise {ExerciseId} created successfully for user {UserId}.", newExercise.Id, userId);
            return CreatedAtAction(nameof(GetExerciseById), new { id = newExercise.Id }, newExercise.ToProgrammedExerciseDto());
        }

        [HttpPut("update/{programId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTrainingProgram([FromRoute] int programId, [FromBody] TrainingProgramUpdateDto programDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is updating training program {ProgramId}.", userId, programId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update training program {ProgramId}.", userId, programId);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.TrainingProgramExists(userId, programId))
            {
                _logger.LogWarning("User {UserId} attempted to update non-existing or unauthorized training program {ProgramId}.", userId, programId);
                return NotFound();
            }
            var updated_program = await _programRepository.UpdateTrainingProgram(programId, programDto);
            if (updated_program == null)
            {
                _logger.LogError("Failed to update training program {ProgramId} for user {UserId}.", programId, userId);
                return NotFound();
            }
            _logger.LogInformation("Training program {ProgramId} updated successfully for user {UserId}.", programId, userId);
            return NoContent();
        }

        [HttpPut("update/days/{dayId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProgramDay([FromRoute] int dayId, [FromBody] ProgramDayUpdateDto dayDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is updating program day {DayId}.", userId, dayId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update program day {DayId}.", userId, dayId);
                return BadRequest(ModelState);
            }
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                _logger.LogWarning("User {UserId} attempted to update non-existing or unauthorized program day {DayId}.", userId, dayId);
                return NotFound();
            }

            var existingDay = await _programRepository.UpdateProgramDay(dayId, dayDto);
            if (existingDay == null)
            {
                _logger.LogError("Failed to update program day {DayId} for user {UserId}.", dayId, userId);
                return NotFound();
            }
            _logger.LogInformation("Program day {DayId} updated successfully for user {UserId}.", dayId, userId);
            return NoContent();
        }

        [HttpPut("update/exercises/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProgrammedExercise([FromRoute] int id, [FromBody] ProgrammedExerciseUpdateDto exerciseDto)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is updating programmed exercise {ExerciseId}.", userId, id);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to update programmed exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }
            
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                _logger.LogWarning("User {UserId} attempted to update non-existing or unauthorized programmed exercise {ExerciseId}.", userId, id);
                return NotFound();
            }
            var existingExercise = await _programRepository.UpdateProgrammedExercise(id, exerciseDto);
            if (existingExercise == null)
            {
                _logger.LogError("Failed to update programmed exercise {ExerciseId} for user {UserId}.", id, userId);
                return NotFound();
            }
            _logger.LogInformation("Programmed exercise {ExerciseId} updated successfully for user {UserId}.", id, userId);
            return NoContent();
        }

        [HttpDelete("delete/{programId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTrainingProgram([FromRoute] int programId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is deleting training program {ProgramId}.", userId, programId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete training program {ProgramId}.", userId, programId);
                return BadRequest(ModelState);
            }
            if (!await _programRepository.TrainingProgramExists(userId, programId))
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existing or unauthorized training program {ProgramId}.", userId, programId);
                return NotFound();
            }
            var days = await _programRepository.GetDaysByProgramId(programId);
            if (days != null)
            {
                foreach (var day in days)
                {
                    if (day == null)
                    {
                        _logger.LogWarning("Null day encountered when user {UserId} attempted to delete training program {ProgramId}.", userId, programId);
                        continue;
                    }
                    _logger.LogInformation("Deleting program day {DayId} for user {UserId}.", day.Id, userId);
                    await _programRepository.DeleteProgramDay(day.Id);
                }
            }
            _logger.LogDebug("Deleting training program {ProgramId} for user {UserId}.", programId, userId);
            await _programRepository.DeleteTrainingProgram(programId);
            _logger.LogInformation("Training program {ProgramId} deleted successfully for user {UserId}.", programId, userId);
            return NoContent();
        }

        [HttpDelete("delete/days/{dayId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProgramDay([FromRoute] int dayId)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is deleting program day {DayId}.", userId, dayId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete program day {DayId}.", userId, dayId);
                return BadRequest(ModelState);
            }
            if (!await _programRepository.ProgramDayExists(userId, dayId))
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existing or unauthorized program day {DayId}.", userId, dayId);
                return NotFound();
            }
            _logger.LogDebug("Deleting program day {DayId} for user {UserId}.", dayId, userId);
            await _programRepository.DeleteProgramDay(dayId);
            _logger.LogInformation("Program day {DayId} deleted successfully for user {UserId}.", dayId, userId);
            return NoContent();
        }

        [HttpDelete("delete/exercises/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProgrammedExercise([FromRoute] int id)
        {
            var userId = User.GetId();
            _logger.LogDebug("User {UserId} is deleting programmed exercise {ExerciseId}.", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when user {UserId} attempted to delete programmed exercise {ExerciseId}.", userId, id);
                return BadRequest(ModelState);
            }
            if (!await _programRepository.ProgrammedExerciseExists(userId, id))
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existing or unauthorized programmed exercise {ExerciseId}.", userId, id);
                return NotFound();
            }
            _logger.LogInformation("Deleting programmed exercise {ExerciseId} for user {UserId}.", id, userId);
            var existingExercise = await _programRepository.DeleteProgrammedExercise(id);
            if (existingExercise == null)
            {
                _logger.LogError("Failed to delete programmed exercise {ExerciseId} for user {UserId}.", id, userId);
                return NotFound();
            }
            _logger.LogInformation("Programmed exercise {ExerciseId} deleted successfully for user {UserId}.", id, userId);
            return NoContent();
        }
    }
}