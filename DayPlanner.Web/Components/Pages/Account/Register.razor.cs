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


    //private readonly RegisterUserModel _model = new() { Password = string.Empty };
    private bool _emailTaken = false;
    private RegisterUserModel _model = new() { Password = string.Empty };
    private bool _confirmPasswordNotVisible = false;
    private bool _passwordNotVisible = false;

    private async Task Form_OnSubmitAsync()
    {
        //await _form.Validate();
        //if (!_form.IsValid)
        //    return;

        //if (string.IsNullOrWhiteSpace(_model.DisplayName))
        //    _model.DisplayName = _model.Email![.._model.Email!.IndexOf('@')];

        (User? user, ApiErrorModel? error) = await AuthenticationService.RegisterAsync(null);
        if (user is null)
        {
            if (error?.Error == "Email is already in use")
            {
                _emailTaken = true;
            }
            else
            {
                //await DialogService.ShowMessageBox(
                //title: Localizer["error_Title"],
                //message: Localizer["error_Message", error?.Error ?? "Unknown"],
                //yesText: Localizer["error_ConfirmBtn"]);
            }
        }
        else
        {
            //NavigationManager.NavigateToDashboard();
        }
    }

    private void FormEmail_OnValueChanged()
    {
        _emailTaken = false;
        //_form.ResetValidation();
    }

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
