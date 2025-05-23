﻿using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Wasm.Models;
using System.Security.Claims;

namespace DayPlanner.Web.Wasm.Extensions;

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
    public static ClaimsPrincipal ToClaimsPrincipial(this UserSession user)
    {
        ArgumentNullException.ThrowIfNull(user);

        List<Claim> claims = [
            new Claim(nameof(UserSession.Uid), user.Uid),
            new Claim(nameof(UserSession.DisplayName), user.DisplayName ?? string.Empty),
            new Claim(nameof(UserSession.Email), user.Email ?? string.Empty),
            new Claim(nameof(UserSession.PhoneNumber), user.PhoneNumber ?? string.Empty),
            new Claim(nameof(UserSession.PhotoUrl), user.PhotoUrl ?? string.Empty),
            new Claim(nameof(UserSession.Token), user.Token ?? string.Empty),
            new Claim(nameof(UserSession.RefreshToken), user.RefreshToken ?? string.Empty),
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
    public static UserSession ToUserSession(this ClaimsPrincipal claimsPrincipal)
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
            Token = claimsPrincipal.FindFirst(nameof(UserSession.Token))!.Value,
            RefreshToken = claimsPrincipal.FindFirst(nameof(UserSession.RefreshToken))!.Value,
            PhoneNumber = claimsPrincipal.FindFirst(nameof(User.PhoneNumber))!.Value,
            PhotoUrl = claimsPrincipal.FindFirst(nameof(User.PhotoUrl))!.Value,
            EmailVerified = emailVerified,
            LastSignInTimestamp = lastSignIn
        };
    }
}
