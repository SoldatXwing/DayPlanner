using FirebaseAdmin.Auth;

namespace DayPlanner.Abstractions.Services
{
    public interface IAuthService
    {
        Task<FirebaseToken> VerifyTokenAsync(string idToken);
    }
}