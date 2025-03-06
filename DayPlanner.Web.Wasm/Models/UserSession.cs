using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Web.Wasm.Models;

internal class UserSession : User
{
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

