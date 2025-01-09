using FirebaseAdmin.Auth;
using FirebaseAdmin;
using DayPlanner.Abstractions.Services;

namespace DayPlanner.Authorization.Services;

public class AuthService(FirebaseApp app) : IAuthService
{
    private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app) ?? throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");

    public async Task<string?> VerifyTokenAsync(string idToken)
    {
        try
        {
            FirebaseToken token = await _firebaseAuth.VerifyIdTokenAsync(idToken).ConfigureAwait(false);
            return token.Uid;
        }
        catch (Exception)
        {
            return null;
        }
    }
}