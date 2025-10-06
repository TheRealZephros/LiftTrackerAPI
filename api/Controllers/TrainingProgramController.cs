using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.TrainingProgram;
using api.Mappers;
using api.Models;
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
        [HttpGet("user/{userId}")]
        public IActionResult GetTrainingProgramsForUser([FromRoute] int userId)
        {
            var programs = _context.TrainingPrograms
                .Where(p => p.UserId == userId)
                .ToList();
            return Ok(programs);
        }
        [HttpGet]
        public IActionResult GetTrainingProgramById([FromBody] TrainingProgramGetByIdDto dto)
        {
            var program = _context.TrainingPrograms.FirstOrDefault(p => p.Id == dto.Id && p.UserId == dto.UserId);
            if (program == null)
            {
                return NotFound();
            }
            return Ok(program);
        }

        [HttpPost("create")]
        public IActionResult CreateTrainingProgram([FromBody] TrainingProgramCreateDto programDto)
        {
            var newProgram = programDto.ToTrainingProgram();
            _context.TrainingPrograms.Add(newProgram);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetTrainingProgramById), new { userId = newProgram.UserId, programId = newProgram.Id }, newProgram);
        }
        [HttpPost("create/day")]
        public IActionResult CreateProgramDay(ProgramDayCreateDto programDayDto)
        {
            var program = _context.TrainingPrograms.FirstOrDefault(p => p.Id == programDayDto.TrainingProgramId);
            if (program == null)
            {
                return NotFound();
            }

            var newProgramDay = programDayDto.ToProgramDay();
            newProgramDay.TrainingProgramId = program.Id;
            _context.ProgramDays.Add(newProgramDay);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTrainingProgramById), new { userId = program.UserId, programId = program.Id }, newProgramDay);
        }
    }
}