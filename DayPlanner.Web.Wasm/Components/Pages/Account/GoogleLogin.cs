﻿using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace DayPlanner.Web.Wasm.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/google/login")]
public sealed class GoogleLogin : ComponentBase
{
    #region Injections
    [Inject]
    private Services.IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    [SupplyParameterFromQuery(Name = "token")]
    private string? Token { get; set; }

    [SupplyParameterFromQuery(Name = "refreshToken")]
    private string? RefreshToken { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(RefreshToken))
        {
            NavigationManager.NavigateToLogin();
            return;
        }

        (UserSession? session, ApiErrorModel? _) = await AuthenticationService.LoginViaGoogleAsync(Token, RefreshToken);
        if (session is null)
        {
            NavigationManager.NavigateToLogin();
        }
        else
        {
            NavigationManager.NavigateToDashboard();
        }
    }
}
