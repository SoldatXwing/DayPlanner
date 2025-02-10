using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Extensions;
using DayPlanner.Web.Models;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace DayPlanner.Web.Components.Pages.Account;

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
    private IMemoryCache Cache { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;
    #endregion

    private bool _passwordNotVisible = true;

    private readonly UserRequest _model = new();

    private bool _loginError = false;

    [SupplyParameterFromQuery(Name = "key")]
    private string? ModelKey { get; set; }

    [SupplyParameterFromQuery(Name = "showError")]
    private bool? ShowError { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    private string? ReturnUrl { get; set; }

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (ModelKey is not null)
        {
            if (HttpContext is null)
            {
                Navigation.Refresh(forceReload: true);
                return;
            }

            if (Cache.TryGetValue(ModelKey, out UserRequest? request))
            {
                Cache.Remove(ModelKey);

                UserSession? session = await AuthenticationService.LoginAsync(request!);
                if (session is null)
                {
                    Navigation.NavigateToLogin(showError: true);
                }
                else
                {
                    await HttpContext.SignInAsync(session.ToClaimsPrincipial());
                    Navigation.NavigateTo(ReturnUrl ?? Routes.Dashboard());
                }
            }
        }

        _loginError = ShowError ?? false;
    }
    private void Form_OnSubmit()
    {
        try
        {
            string key = Guid.NewGuid().ToString();

            using ICacheEntry _ = Cache.CreateEntry(key)
                .SetValue(_model)
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            Navigation.NavigateToLogin(key: key, forceLoad: true);
        }
        catch
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
