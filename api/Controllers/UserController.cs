using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Interfaces;
using api.Data;
using api.Dtos.User;

namespace api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            ApplicationDbContext context,
            ILogger<UserController> logger
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _context = context;
            _logger = logger;
        }

        // ---------------------------------------
        // LOGIN
        // ---------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            _logger.LogDebug("Login attempt for email {Email}.", dto.Email);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: unknown email.");
                return Unauthorized("Invalid email or password");
            }

            var userId = user.Id;
            _logger.LogDebug("User {userId} located for login.", userId);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: invalid password for user {userId}.", userId);
                return Unauthorized("Invalid email or password");
            }

            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {userId} logged in successfully.", userId);

            return Ok(new
            {
                accessToken,
                refreshToken,
                user = new { user.UserName, user.Email }
            });
        }

        // ---------------------------------------
        // REFRESH TOKEN
        // ---------------------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            _logger.LogDebug("Refresh token request received.");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token request missing token.");
                return BadRequest("Refresh token cannot be empty.");
            }

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token.");
                return Unauthorized("Invalid or expired refresh token.");
            }

            var userId = storedToken.UserId;
            _logger.LogDebug("Refresh token validated for user {userId}.", userId);

            var newAccessToken = _tokenService.CreateAccessToken(storedToken.User);

            _logger.LogInformation("New access token issued for user {userId}.", userId);

            return Ok(new { accessToken = newAccessToken });
        }

        // ---------------------------------------
        // REGISTER
        // ---------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration failed: invalid model state.");
                return BadRequest(ModelState);
            }

            _logger.LogDebug("Registration model validated for email {Email}.", dto.Email);

            try
            {
                var newUser = new User
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                };

                var created = await _userManager.CreateAsync(newUser, dto.Password);
                if (!created.Succeeded)
                {
                    _logger.LogWarning("User creation failed during registration.");
                    return StatusCode(500, "Failed to create user.");
                }

                var userId = newUser.Id;
                _logger.LogInformation("User created successfully. userId={userId}", userId);

                var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Role assignment failed for user {userId}, rolling back.", userId);
                    await _userManager.DeleteAsync(newUser);
                    return StatusCode(500, "Failed to assign role.");
                }

                _logger.LogDebug("Role 'User' assigned to user {userId}.", userId);

                var accessToken = _tokenService.CreateAccessToken(newUser);
                var refreshToken = _tokenService.CreateRefreshToken();

                _context.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });

                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration completed successfully for user {userId}.", userId);

                return Ok(new
                {
                    user = new { newUser.UserName, newUser.Email },
                    accessToken,
                    refreshToken
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception during registration.");
                return StatusCode(500, "Unexpected server error.");
            }
        }

        // ---------------------------------------
        // DELETE USER
        // ---------------------------------------
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("Attempting user deletion. userId={userId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User deletion failed: user {userId} not found.", id);
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("User deletion failed for {userId}.", id);
                return StatusCode(500, "Failed to delete user.");
            }

            _logger.LogInformation("User {userId} deleted successfully.", id);

            return NoContent();
        }
    }
}
