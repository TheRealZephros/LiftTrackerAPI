using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Dtos.User;
using api.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using api.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newUser = new User
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                };
                var createdUser = await _userManager.CreateAsync(newUser, dto.Password);
                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok(new UserRegisterReturnDto
                        {
                            UserName = newUser.UserName,
                            Email = newUser.Email,
                            Token = _tokenService.CreateToken(newUser)
                        });
                    }
                    else
                    {
                        await _userManager.DeleteAsync(newUser);
                        return StatusCode(500, "Failed to assign role to user.");
                    }
                }
                else
                {
                    return StatusCode(500, "Failed to create user.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password.");

            return Ok(new UserLoginReturnDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, "Failed to delete user.");

            return NoContent();
        }
    }
}
