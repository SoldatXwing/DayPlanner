namespace DayPlanner.Abstractions.Services
{
    public interface IJwtProvider
    {
        Task<string> GetForCredentialsAsync(string email, string password);
    }
}