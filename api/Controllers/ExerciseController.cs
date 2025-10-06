using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Exercise;
using api.Mappers;
using api.Models;
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
            var exercises = _context.Exercises
                .Where(e => !e.IsUsermade || e.UserId == userId)
                .ToList()
                .Select(e => e.ToExerciseDto());
            
            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public IActionResult GetExerciseById(int id)
        {
            var exercise = _context.Exercises.FirstOrDefault(e => e.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise.ToExerciseDto());
        }

        [HttpPost("create")]
        public IActionResult CreateExercise([FromBody] ExerciseDto exercise)
        {
            var newExercise = exercise.ToExercise();
            _context.Exercises.Add(newExercise);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetExerciseById), new { id = newExercise.Id }, newExercise);
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateExercise(int id, [FromBody] ExerciseDto exercise)
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
            if (existingExercise == null || existingExercise.UserId != exercise.Id)
            {
                return NotFound();
            }
            _context.Exercises.Remove(existingExercise);
            _context.SaveChanges();
            return NoContent();
        }
    }
}