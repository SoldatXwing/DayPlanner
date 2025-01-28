using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace DayPlanner.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/login")]
public sealed partial class Login : ComponentBase
{
    #region Injections
    [Inject]
    private IStringLocalizer<Login> Localizer { get; set; } = default!;

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    private MudForm _form = default!;
    private MudTextField<string> _emailField = default!;

    private readonly UserRequest _model = new();
    private bool _loginError = false;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private async Task Form_OnSubmitAsync()
    {
        await _emailField.Validate();     // Only email and pwd field existing and pwd can only be validated by login attempt
        if (_emailField.ValidationErrors.Count == 0)
        {
            User? user = await AuthenticationService.LoginAsync(_model);
            if (user is not null)
            {
                NavigationManager.NavigateToDashboard();
            }
            else
            {
                _loginError = true;
            }
        }
    }

    private async Task GoogleLogin_OnClickAsync()
    {
        string authUrl = await AuthenticationService.GetGoogleAuthUrlAsync();
        await Browser.Default.OpenAsync(authUrl.Trim('"'), BrowserLaunchMode.SystemPreferred);
    }
    private void OnInputChanged()
    {
        _loginError = false;
        _form.ResetValidation();
    }
    protected async override Task OnAfterRenderAsync(bool firstRender) // Primary needed for google sign in
    {
        if (firstRender)
        {
            bool isLoggedIn = await AuthenticationService.IsLoggedInAsync(); // Directly nesting in if isnt working
            if (isLoggedIn)
            {
                NavigationManager.NavigateToDashboard();
            }
        }
    }
}
