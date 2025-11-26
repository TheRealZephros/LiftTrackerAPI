using api.Data;
using api.Infrastructure.Auditing;
using api.Infrastructure.Correlation;
using api.Infrastructure.Security;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace api.Tests.Infrastructure.Auditing
{
    public static class AuditInterceptorTestHelpers
    {
        public static Mock<ICorrelationIdAccessor> MockCorrelation(string correlationId = "test-correlation-id")
        {
            var mock = new Mock<ICorrelationIdAccessor>();
            mock.Setup(c => c.CorrelationId).Returns(correlationId);
            return mock;
        }

        public static Mock<IUserContext> MockUserContext(
            string userId = "user-1",
            string userName = "testuser",
            string email = "test@example.com")
        {
            var mock = new Mock<IUserContext>();
            mock.Setup(u => u.UserId).Returns(userId);
            mock.Setup(u => u.UserName).Returns(userName);
            mock.Setup(u => u.Email).Returns(email);
            return mock;
        }

        public static Mock<IHttpContextInfo> MockHttpContextInfo(
            string ipAddress = "127.0.0.1",
            string userAgent = "UnitTestAgent/1.0")
        {
            var mock = new Mock<IHttpContextInfo>();
            mock.Setup(h => h.IpAddress).Returns(ipAddress);
            mock.Setup(h => h.UserAgent).Returns(userAgent);
            return mock;
        }

        public static AuditDbContext CreateAuditDbContext()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AuditDbContext(options);
        }

        public static ApplicationDbContext CreateAppDbContext(AuditSaveChangesInterceptor interceptor)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            return new ApplicationDbContext(options);
        }

        public static Exercise CreateSampleExercise(string name = "Sample Exercise")
        {
            return new Exercise
            {
                Name = name,
                Description = "Sample description",
                IsUsermade = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
