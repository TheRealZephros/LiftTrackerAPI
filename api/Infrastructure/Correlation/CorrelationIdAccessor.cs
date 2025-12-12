using Api.Interfaces;

namespace Api.Infrastructure.Correlation
{
    public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? CorrelationId =>
            _httpContextAccessor.HttpContext?
                .Request
                .Headers["X-Correlation-ID"]
                .FirstOrDefault();
    }
}