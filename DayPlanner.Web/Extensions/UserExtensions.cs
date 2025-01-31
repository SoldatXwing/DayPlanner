using DayPlanner.Abstractions.Models.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Web.Extensions;

internal static class UserExtensions
{
    public static ClaimsPrincipal ToClaimsPrincipial(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        List<Claim> claims = [
            new Claim(nameof(User.Uid), user.Uid),
            new Claim(nameof(User.DisplayName), user.DisplayName ?? string.Empty),
            new Claim(nameof(User.Email), user.Email ?? string.Empty),
            new Claim(nameof(User.PhoneNumber), user.PhoneNumber ?? string.Empty),
            new Claim(nameof(User.PhotoUrl), user.PhotoUrl ?? string.Empty)
        ];

        if (user.EmailVerified is not null)
            claims.Add(new(nameof(User.EmailVerified), user.EmailVerified.ToString()!));

        if (user.LastSignInTimestamp is not null)
            claims.Add(new(nameof(User.LastSignInTimestamp), user.LastSignInTimestamp.ToString()!));

        return new(new ClaimsIdentity(claims, authenticationType: "API"));
    }

    public static User ToUser(this ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        bool? emailVerified = null;
        if (claimsPrincipal.FindFirst(nameof(User.EmailVerified)) is Claim verifiedClaim)
            emailVerified = bool.Parse(verifiedClaim.Value);

        DateTime? lastSignIn = null;
        if (claimsPrincipal.FindFirst(nameof(User.LastSignInTimestamp)) is Claim lastSignInClaim)
            lastSignIn = DateTime.Parse(lastSignInClaim.Value);

        return new()
        {
            Uid = claimsPrincipal.FindFirst(nameof(User.Uid))!.Value,
            DisplayName = claimsPrincipal.FindFirst(nameof(User.DisplayName))!.Value,
            Email = claimsPrincipal.FindFirst(nameof(User.Email))!.Value,
            PhoneNumber = claimsPrincipal.FindFirst(nameof(User.PhoneNumber))!.Value,
            PhotoUrl = claimsPrincipal.FindFirst(nameof(User.PhotoUrl))!.Value,
            EmailVerified = emailVerified,
            LastSignInTimestamp = lastSignIn
        };
    }
}
