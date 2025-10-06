using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Dtos.User;
using api.Models;
using System.Linq;
using BCrypt.Net;

namespace api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UserController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult RegisterUser([FromBody] UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var newUser = new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { UserId = newUser.Id });
        }

        [HttpPost("login")]
        public IActionResult LoginUser([FromBody] UserLoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password.");

            return Ok(new { UserId = user.Id });
        }

        [HttpDelete("delete")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
