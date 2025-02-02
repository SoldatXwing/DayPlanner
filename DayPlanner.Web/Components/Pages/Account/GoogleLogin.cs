using DayPlanner.Web.Refit;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Dynamic.Core.Tokenizer;
namespace DayPlanner.Web.Components.Pages.Account
{
    [Route("/account/google/login")]
    public sealed class GoogleLogin : ComponentBase
    {
        #region Injections
        [Inject]
        private IAuthenticationService AuthenticationService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        #endregion

        private string? GetQueryParameter(string key)
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return queryParams[key] ?? null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                string? token = GetQueryParameter("token");
                string? refreshToken = GetQueryParameter("refreshToken");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    NavigationManager.NavigateToHome();
                    return;
                }
                try
                {
                    await AuthenticationService.LoginViaGoogleAsync(token);
                    NavigationManager.NavigateToDashboard();
                }
                catch
                {
                    NavigationManager.NavigateToHome();
                }

            }

        }

    }
}
