using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Models;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace DayPlanner.Web.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/register")]
public sealed partial class Register : ComponentBase
{
    #region Injections
    [Inject]
    private IStringLocalizer<Register> Localizer { get; set; } = default!;

    [Inject]
    private IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion


    private string _errorMessage = string.Empty;
    private RegisterUserModel _model = new() { Password = string.Empty };
    private bool _confirmPasswordNotVisible = true;
    private bool _passwordNotVisible = true;

    private async Task Form_OnSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.DisplayName))
            _model.DisplayName = _model.Email![.._model.Email!.IndexOf('@')];

        (User? user, ApiErrorModel? error) = await AuthenticationService.RegisterAsync(_model);
        if (user is null)
        {
            if (error?.Error == "Email is already in use")
            {
                _errorMessage = Localizer["form_Email_Taken"];
            }
            else
            {
                _errorMessage = Localizer["error_Message", error?.Error ?? "Unknown"];
            }
        }
        else
        {
            //NavigationManager.NavigateToDashboard();
        }
    }

    private void ChangeErrorState() => _errorMessage = string.Empty;

    private string? ValidatePassword(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length < 6     // is null or empty check because the if not the required error isn't shown.
            ? Localizer["form_Password_TooShort", 7].ToString()
            : null;
    }

    private string? ValidateConfirmedPassword(string password)
    {
        //return !string.IsNullOrEmpty(password) && password != _model.Password     // is null or empty check because the if not the required error isn't shown.
        //    ? Localizer["form_ConfirmPassword_Invalid"].ToString()
        //    : null;
        return "";
    }
}
