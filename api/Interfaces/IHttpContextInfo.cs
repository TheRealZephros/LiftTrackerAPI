namespace Api.Interfaces
{
    public interface IHttpContextInfo
    {
        string? IpAddress { get; }
        string? UserAgent { get; }
    }
}
