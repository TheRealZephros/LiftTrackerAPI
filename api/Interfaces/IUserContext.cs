namespace api.Interfaces
{
    public interface IUserContext
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
    }
}
