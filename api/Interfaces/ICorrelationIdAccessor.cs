namespace api.Interfaces
{
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; }
    }
}
