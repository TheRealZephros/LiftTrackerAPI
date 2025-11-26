using System.Threading.Tasks;
using api.Data;
using api.Models;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Repositories
{
    public class UserRepositoryTests
    {
        #region Test Setup

        private async Task<ApplicationDbContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        #endregion

        #region CreateUser

        [Fact]
        public async Task CreateUser_ShouldAddUser()
        {
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            var user = new User
            {
                Id = "user123",
                Email = "test@example.com",
                FullName = "Test User"
            };

            var result = await repo.CreateUser(user, "password123");

            Assert.NotNull(result);
            Assert.Equal("user123", result!.Id);

            var fromDb = await context.Users.FindAsync("user123");
            Assert.NotNull(fromDb);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
        {
            var context = await GetDbContextAsync();
            context.Users.Add(new User { Id = "abc", Email = "a@a.com" });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var user = await repo.GetByIdAsync("abc");

            Assert.NotNull(user);
            Assert.Equal("abc", user!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            var result = await repo.GetByIdAsync("missing");

            Assert.Null(result);
        }

        #endregion

        #region GetByEmailAsync

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailMatches()
        {
            var context = await GetDbContextAsync();
            context.Users.Add(new User { Id = "u1", Email = "test@mail.com" });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetByEmailAsync("test@mail.com");

            Assert.NotNull(result);
            Assert.Equal("u1", result!.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotMatch()
        {
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            var result = await repo.GetByEmailAsync("nope@mail.com");

            Assert.Null(result);
        }

        #endregion

        #region DeleteUser

        [Fact]
        public async Task DeleteUser_ShouldRemoveUser_WhenExists()
        {
            var context = await GetDbContextAsync();
            context.Users.Add(new User { Id = "delete-me", Email = "d@m.com" });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var deleted = await repo.DeleteUser("delete-me");

            Assert.NotNull(deleted);
            Assert.Equal("delete-me", deleted!.Id);

            var fromDb = await context.Users.FindAsync("delete-me");
            Assert.Null(fromDb);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNull_WhenNotExists()
        {
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            var deleted = await repo.DeleteUser("missing");

            Assert.Null(deleted);
        }

        #endregion

        #region UserExists

        [Fact]
        public async Task UserExists_ShouldReturnTrue_WhenUserExists()
        {
            var context = await GetDbContextAsync();
            context.Users.Add(new User { Id = "exists" });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var exists = await repo.UserExists("exists");

            Assert.True(exists);
        }

        [Fact]
        public async Task UserExists_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            var exists = await repo.UserExists("nope");

            Assert.False(exists);
        }

        #endregion
    }
}
