using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Dtos.User;
using api.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using api.Interfaces;

namespace api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                return BadRequest("Email already exists.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var newUser = new User
            {
                Email = dto.Email,
                PasswordHash = hashedPassword
            };
            await _userRepository.CreateUser(newUser);
            return Ok(new { UserId = newUser.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return Ok(new { UserId = user.Id });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userRepository.DeleteUser(id);
            if (user == null)
                return NotFound();
            return NoContent();

        }
    }
}
