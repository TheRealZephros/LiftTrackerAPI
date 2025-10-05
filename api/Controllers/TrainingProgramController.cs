using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/program")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public TrainingProgramController(ApplicationDBContext context)
        {
            _context = context;
        }
        [HttpGet("{userId}")]
        public IActionResult GetTrainingProgramsForUser(int userId)
        {
            var programs = _context.TrainingPrograms
                .Where(p => p.UserId == userId)
                .ToList();
            return Ok(programs);
        }
        [HttpGet("{userId}/{programId}")]
        public IActionResult GetTrainingProgramById(int userId, int programId)
        {
            var program = _context.TrainingPrograms.FirstOrDefault(p => p.Id == programId && p.UserId == userId);
            if (program == null)
            {
                return NotFound();
            }
            return Ok(program);
        }
    }
}