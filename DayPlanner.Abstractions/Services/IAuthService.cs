using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Services
{
    public interface IAuthService
    {
        Task<FirebaseToken> VerifyTokenAsync(string idToken);
        Task<UserRecord> GetUserByIdAsync(string uid);
        Task<UserRecord> CreateUserAsync(UserRecordArgs args);
    }
}