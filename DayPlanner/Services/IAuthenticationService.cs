using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Services;

internal interface IAuthenticationService
{
    public Task<User?> LoginAsync(UserRequest request);

    public Task<(User? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request);
}
