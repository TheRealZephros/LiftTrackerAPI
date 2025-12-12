using api.Data;
using api.Infrastructure.Auditing;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Helpers.Infrastructure.Auditing
{
    public static class AuditInterceptorTestHelpers
    {
        public static Mock<ICorrelationIdAccessor> MockCorr(string id = "cid")
        {
            var m = new Mock<ICorrelationIdAccessor>();
            m.Setup(x => x.CorrelationId).Returns(id);
            return m;
        }

        public static Mock<IUserContext> MockUser(string id = "u1", string name = "user", string mail = "u@x.com")
        {
            var m = new Mock<IUserContext>();
            m.Setup(x => x.UserId).Returns(id);
            m.Setup(x => x.UserName).Returns(name);
            m.Setup(x => x.Email).Returns(mail);
            return m;
        }

        public static Mock<IHttpContextInfo> MockHttp(string ip = "127.0.0.1", string ua = "unit")
        {
            var m = new Mock<IHttpContextInfo>();
            m.Setup(x => x.IpAddress).Returns(ip);
            m.Setup(x => x.UserAgent).Returns(ua);
            return m;
        }

        public static (ApplicationDbContext app, AuditDbContext audit, AuditSaveChangesInterceptor interceptor)
            CreateDbContextsWithInterceptor()
        {
            string appDbName = Guid.NewGuid().ToString();
            string auditDbName = Guid.NewGuid().ToString();

            var auditOptions = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase(auditDbName)
                .Options;

            var auditDb = new AuditDbContext(auditOptions);

            var interceptor = new AuditSaveChangesInterceptor(
                MockCorr().Object,
                MockUser().Object,
                MockHttp().Object,
                auditDb
            );

            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(appDbName)
                .AddInterceptors(interceptor)
                .Options;

            var appDb = new ApplicationDbContext(appOptions);

            return (appDb, auditDb, interceptor);
        }

        public static User CreateSampleUser(string name, string email)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = name,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
