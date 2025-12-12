namespace Api.Interfaces
{
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; }
    }
}
