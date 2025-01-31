using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen;
using Radzen.Blazor;
using System.ComponentModel.DataAnnotations;

namespace DayPlanner.Web.Components.Pages.Account;

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
    private NavigationManager Navigation { get; set; } = default!;
    #endregion

    private bool _passwordNotVisible = true;
    private UserRequest _model = new UserRequest();
    private async Task Form_OnSubmitAsync()
    {
        if (true)
        {       
            User? user = await AuthenticationService.LoginAsync(null);

            if (user is not null)
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                //_loginError = true;
            }
        }
    }

    private async Task GoogleLogin_OnClickAsync()
    {
        //await AuthenticationService.LoginViaGoogleAsync();
    }

    private void OnInputChanged(object value)
    {
        //_loginError = false; // Fehleranzeige zurücksetzen, wenn der Nutzer etwas eingibt
    }
}
