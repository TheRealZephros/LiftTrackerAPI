using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.ExerciseSession;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/session")]
    [ApiController]
    public class ExerciseSessionController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public ExerciseSessionController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetExerciseSessionsForUser(int userId)
        {
            var sessions = _context.ExerciseSessions
                .Where(s => s.UserId == userId)
                .ToList();
            return Ok(sessions);
        }

        [HttpGet("{sessionId}")]
        public IActionResult GetExerciseSessionById([FromRoute] int sessionId)
        {
            var session = _context.ExerciseSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session.ToExerciseSessionDto());
        }

        [HttpPost("create")]
        public IActionResult CreateExerciseSession([FromBody] ExerciseSessionCreateDto dto)
        {
            var newSession = new ExerciseSession
            {
                ExerciseId = dto.ExerciseId,
                UserId = dto.UserId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ExerciseSessions.Add(newSession);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetExerciseSessionById), new { sessionId = newSession.Id }, newSession.ToExerciseSessionDto());
        }

        

    }
}