using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Models;
using DayPlanner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace DayPlanner.Components.Pages;

[AllowAnonymous]
[Route("/register")]
public sealed partial class Register : ComponentBase
{
    #region Injections
    [Inject]
    private IStringLocalizer<Register> Localizer { get; set; } = default!;

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    private MudForm _form = default!;

    private readonly RegisterUserModel _model = new() { Password = string.Empty };
    private bool _emailTaken = false;

    private async Task Form_OnSubmitAsync()
    {
        await _form.Validate();
        if (!_form.IsValid)
            return;

        (User? user, ApiErrorModel? error) = await AuthenticationService.RegisterAsync(_model);
        if (user is null)
        {
            if (error?.Error == "Email is already in use")
            {
                _emailTaken = true;
            }
            else
            {
                await DialogService.ShowMessageBox(
                    title: Localizer["error_Title"],
                    message: Localizer["error_Message", error?.Error ?? "Unknown"],
                    yesText: Localizer["error_ConfirmBtn"]);
            }
        }
        else
        {
            NavigationManager.NavigateToDashboard();
        }
    }

    private void FormEmail_OnValueChanged()
    {
        _emailTaken = false;
        _form.ResetValidation();
    }

    private string? ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length < 6     // is null or empty check because the if not the required error isn't shown.
            ? Localizer["form_Password_TooShort", 7].ToString()
            : null;
    }

    private string? ValidateConfirmedPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password != _model.Password     // is null or empty check because the if not the required error isn't shown.
            ? Localizer["form_ConfirmPassword_Invalid"].ToString()
            : null;
    }
}
