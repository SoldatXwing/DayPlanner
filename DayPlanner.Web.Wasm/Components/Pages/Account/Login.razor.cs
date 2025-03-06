using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace DayPlanner.Web.Wasm.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/login")]
public sealed partial class Login : ComponentBase
{
    #region Injections
    [Inject]
    private IStringLocalizer<Login> Localizer { get; set; } = default!;

    [Inject]
    private Services.IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;
    #endregion

    private bool _passwordNotVisible = true;

    private readonly UserRequest _model = new();

    private bool _loginError = false;

    [SupplyParameterFromQuery(Name = "showError")]
    private bool? ShowError { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    private string? ReturnUrl { get; set; }

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _loginError = ShowError ?? false;
    }
    private async Task Form_OnSubmit()
    {
        try
        {
            var session = await AuthenticationService.LoginAsync(_model);
            if(session is not null)
            {
                Navigation.NavigateToDashboard();
                return;
            }
            _loginError = true;
            return;
        }
        catch(Exception ex)
        {
        }
    }
    private async Task GoogleLogin_OnClickAsync()
    {
        string url = await AuthenticationService.GetGoogleAuthUrlAsync();
        Navigation.NavigateTo(url.Trim('"'), forceLoad: true);
    }

    private void ChangeErrorState() => _loginError = false;
}
