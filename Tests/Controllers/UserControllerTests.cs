using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Controllers;
using api.Data;
using api.Dtos.User;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Controllers
{
    public class UserControllerTests
    {
        #region Helpers

        private static UserController CreateController(
            Mock<UserManager<User>> userManager = null,
            Mock<SignInManager<User>> signInManager = null,
            Mock<ITokenService> tokenService = null,
            ApplicationDbContext context = null)
        {
            userManager ??= MockUserManager();
            signInManager ??= MockSignInManager(userManager.Object);
            tokenService ??= new Mock<ITokenService>();
            context ??= new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var logger = Mock.Of<ILogger<UserController>>();

            return new UserController(
                userManager.Object,
                signInManager.Object,
                tokenService.Object,
                context,
                logger
            );
        }

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<User>> MockSignInManager(UserManager<User> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(
                userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        #endregion

        #region LOGIN

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((User)null);

            var controller = CreateController(userManager: userManager);

            var result = await controller.Login(new UserLoginDto { Email = "x@y.com", Password = "pass" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid email or password", unauthorized.Value);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
        {
            var user = new User { Id = "1", Email = "x@y.com", UserName = "u" };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(user.Email))
                       .ReturnsAsync(user);

            var signInManager = MockSignInManager(userManager.Object);
            signInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, "wrong", false))
                         .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var tokenService = new Mock<ITokenService>();

            var controller = CreateController(userManager, signInManager, tokenService);

            var result = await controller.Login(new UserLoginDto { Email = user.Email, Password = "wrong" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid email or password", unauthorized.Value);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordNull()
        {
            var user = new User { Id = "1", Email = "x@y.com", UserName = "u" };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            var signInManager = MockSignInManager(userManager.Object);
            signInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, null, false))
                         .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = CreateController(userManager: userManager, signInManager: signInManager);

            var result = await controller.Login(new UserLoginDto { Email = user.Email, Password = null });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid email or password", unauthorized.Value);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithTokensAndUserInfo()
        {
            // Arrange
            var dto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "password"
            };

            var testUser = new User
            {
                Id = "123",
                UserName = "testuser",
                Email = "test@example.com"
            };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(dto.Email))
                    .ReturnsAsync(testUser);

            var signInManager = MockSignInManager(userManager.Object);
            signInManager.Setup(sm => sm.CheckPasswordSignInAsync(testUser, dto.Password, false))
                        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(ts => ts.CreateAccessToken(testUser))
                        .Returns("ACCESS123");
            tokenService.Setup(ts => ts.CreateRefreshToken())
                        .Returns("REFRESH123");

            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var controller = CreateController(userManager, signInManager, tokenService, context);

            // Act
            var result = await controller.Login(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);

            // The controller returns an anonymous object => dynamic
            dynamic response = ok.Value;

            Assert.Equal("ACCESS123", (string)response.AccessToken);
            Assert.Equal("REFRESH123", (string)response.RefreshToken);

            Assert.Equal("testuser", (string)response.User.UserName);
            Assert.Equal("test@example.com", (string)response.User.Email);
        }


        #endregion

        #region REFRESH TOKEN

        [Fact]
        public async Task Refresh_ReturnsBadRequest_WhenTokenEmpty()
        {
            var controller = CreateController();
            var result = await controller.Refresh("");
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Refresh token cannot be empty.", badRequest.Value);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenTokenInvalidOrRevokedOrExpired()
        {
            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            // Invalid token
            var resultInvalid = await CreateController(context: context).Refresh("nonexistent");
            Assert.IsType<UnauthorizedObjectResult>(resultInvalid);

            // Revoked token
            context.RefreshTokens.Add(new RefreshToken
            {
                Token = "revokedToken",
                UserId = "1",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = true
            });
            await context.SaveChangesAsync();

            var resultRevoked = await CreateController(context: context).Refresh("revokedToken");
            Assert.IsType<UnauthorizedObjectResult>(resultRevoked);

            // Expired token
            context.RefreshTokens.Add(new RefreshToken
            {
                Token = "expiredToken",
                UserId = "1",
                ExpiresAt = DateTime.UtcNow.AddSeconds(-1),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            var resultExpired = await CreateController(context: context).Refresh("expiredToken");
            Assert.IsType<UnauthorizedObjectResult>(resultExpired);
        }

        [Fact]
        public async Task Refresh_ReturnsOk_WhenTokenValid()
        {
            var user = new User { Id = "1", Email = "x@y.com", UserName = "u" };
            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            context.RefreshTokens.Add(new RefreshToken
            {
                Token = "validToken",
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                User = user
            });
            await context.SaveChangesAsync();

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(ts => ts.CreateAccessToken(user)).Returns("newAccessToken");

            var controller = CreateController(tokenService: tokenService, context: context);

            var result = await controller.Refresh("validToken");

            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic value = ok.Value;
            Assert.Equal("newAccessToken", value.AccessToken);
        }

        #endregion

        #region REFRESH TOKEN PERSISTENCE

        [Fact]
        public async Task Login_SavesRefreshTokenToDb()
        {
            var user = new User { Id = "1", Email = "x@y.com", UserName = "u" };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(user.Email)).ReturnsAsync(user);

            var signInManager = MockSignInManager(userManager.Object);
            signInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, "pass", false))
                        .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(ts => ts.CreateAccessToken(user)).Returns("accessToken");
            tokenService.Setup(ts => ts.CreateRefreshToken()).Returns("refreshToken");

            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            var controller = CreateController(userManager, signInManager, tokenService, context);

            var result = await controller.Login(new UserLoginDto { Email = user.Email, Password = "pass" });

            // Check DB
            var savedToken = await context.RefreshTokens.FirstOrDefaultAsync();
            Assert.NotNull(savedToken);
            Assert.Equal(user.Id, savedToken.UserId);
            Assert.Equal("refreshToken", savedToken.Token);
            Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task RegisterUser_SavesRefreshTokenToDb()
        {
            var userManager = MockUserManager();
            var newUser = new User { Id = "1", Email = "x@y.com", UserName = "u" };

            userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<User, string>((u, p) => u.Id = newUser.Id);

            userManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "User"))
                    .ReturnsAsync(IdentityResult.Success);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(ts => ts.CreateAccessToken(It.IsAny<User>())).Returns("accessToken");
            tokenService.Setup(ts => ts.CreateRefreshToken()).Returns("refreshToken");

            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            var controller = CreateController(userManager: userManager, tokenService: tokenService, context: context);

            var dto = new UserRegisterDto { Email = "x@y.com", Password = "pass", UserName = "u" };

            var result = await controller.RegisterUser(dto);

            var savedToken = await context.RefreshTokens.FirstOrDefaultAsync();
            Assert.NotNull(savedToken);
            Assert.Equal("refreshToken", savedToken.Token);
            Assert.Equal(newUser.Id, savedToken.UserId);
            Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
        }

        #endregion

        #region REGISTER

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenModelInvalid()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("x", "invalid");

            var result = await controller.RegisterUser(new UserRegisterDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RegisterUser_Returns500_WhenCreationFails()
        {
            var userManager = MockUserManager();
            userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Failed());

            var controller = CreateController(userManager: userManager);

            var result = await controller.RegisterUser(new UserRegisterDto
            {
                Email = "x@y.com",
                Password = "pass",
                UserName = "u"
            });

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_Returns500_WhenRoleAssignmentFails()
        {
            var userManager = MockUserManager();
            var newUser = new User { Id = "1", Email = "x@y.com", UserName = "u" };
            userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success)
                       .Callback<User, string>((u, p) => u.Id = newUser.Id);

            userManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "User"))
                       .ReturnsAsync(IdentityResult.Failed());

            var tokenService = new Mock<ITokenService>();
            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            var controller = CreateController(userManager: userManager, tokenService: tokenService, context: context);

            var dto = new UserRegisterDto { Email = "x@y.com", Password = "pass", UserName = "u" };

            var result = await controller.RegisterUser(dto);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
            userManager.Verify(um => um.DeleteAsync(It.Is<User>(u => u.Id == "1")), Times.Once);
        }

        [Fact]
        public async Task RegisterUser_ReturnsOk_WhenSuccessful_AndSavesRefreshToken()
        {
            var userManager = MockUserManager();
            var newUser = new User { Id = "1", Email = "x@y.com", UserName = "u" };
            userManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                       .ReturnsAsync(IdentityResult.Success)
                       .Callback<User, string>((u, p) => u.Id = newUser.Id);

            userManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "User"))
                       .ReturnsAsync(IdentityResult.Success);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(ts => ts.CreateAccessToken(It.IsAny<User>())).Returns("accessToken");
            tokenService.Setup(ts => ts.CreateRefreshToken()).Returns("refreshToken");

            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
            );

            var controller = CreateController(userManager: userManager, tokenService: tokenService, context: context);

            var dto = new UserRegisterDto { Email = "x@y.com", Password = "pass", UserName = "u" };

            var result = await controller.RegisterUser(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic value = ok.Value;
            Assert.Equal("accessToken", value.AccessToken);
            Assert.Equal("refreshToken", value.RefreshToken);
            Assert.Equal("x@y.com", value.User.Email);
            Assert.Equal("u", value.User.UserName);

            var savedToken = await context.RefreshTokens.FirstOrDefaultAsync();
            Assert.NotNull(savedToken);
            Assert.Equal("refreshToken", savedToken.Token);
            Assert.Equal(newUser.Id, savedToken.UserId);
            Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
        }

        #endregion

        #region DELETE USER

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync((User)null);

            var controller = CreateController(userManager: userManager);

            var result = await controller.DeleteUser("1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenSuccessful()
        {
            var user = new User { Id = "1" };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            userManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManager: userManager);

            var result = await controller.DeleteUser("1");

            Assert.IsType<NoContentResult>(result);
            userManager.Verify(um => um.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_Returns500_WhenDeletionFails()
        {
            var user = new User { Id = "1" };

            var userManager = MockUserManager();
            userManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            userManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed());

            var controller = CreateController(userManager: userManager);

            var result = await controller.DeleteUser("1");

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }
        
        #endregion
    }
}
