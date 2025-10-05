using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data;
using api.models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/exercise")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public ExerciseController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllExercises(int userId)
        {
            var exercises = _context.Exercises.Where(e => !e.IsUsermade || e.UserId == userId).ToList();
            return Ok(exercises);
        }
        [HttpPost("create")]
        public IActionResult CreateExercise([FromBody] CreateExerciseDto exercise)
        {
            var newExercise = new Exercise
            {
                Name = exercise.Name,
                Description = exercise.Description,
                IsUsermade = exercise.IsUsermade,
                UserId = exercise.UserId
            };
            _context.Exercises.Add(newExercise);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetAllExercises), new { id = newExercise.Id }, newExercise);
        }
        [HttpPut("update/{id}")]
        public IActionResult UpdateExercise(int id, [FromBody] UpdateExerciseDto exercise)
        {

            var existingExercise = _context.Exercises.FirstOrDefault(e => e.Id == id);
            if (existingExercise == null || existingExercise.UserId != exercise.UserId)
            {
                return NotFound();
            }
            existingExercise.Name = exercise.Name;
            existingExercise.Description = exercise.Description;
            existingExercise.IsUsermade = exercise.IsUsermade;
            existingExercise.UserId = exercise.UserId;

            _context.SaveChanges();
            return NoContent();
        }
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteExercise(int id, [FromBody] DeleteExerciseDto exercise)
        {
            var existingExercise = _context.Exercises.FirstOrDefault(e => e.Id == id);
            if (existingExercise == null || existingExercise.UserId != exercise.UserId)
            {
                return NotFound();
            }
            _context.Exercises.Remove(existingExercise);
            _context.SaveChanges();
            return NoContent();
        }
    }

    public class CreateExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUsermade { get; set; } = true;
        public int? UserId { get; set; } // Nullable for predefined exercises
    }

    public class UpdateExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUsermade { get; set; } = true;
        public int? UserId { get; set; } // Nullable for predefined exercises
    }
    public class DeleteExerciseDto
    {
        public int? UserId { get; set; } // Nullable for predefined exercises
    }
}