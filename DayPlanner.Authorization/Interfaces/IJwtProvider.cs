namespace DayPlanner.Authorization.Interfaces
{
    public interface IJwtProvider
    {
        Task<string> GetForCredentialsAsync(string email, string password);
    }
}
