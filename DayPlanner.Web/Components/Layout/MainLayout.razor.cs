using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Localization;
using Radzen;
using Radzen.Blazor;
using System.Security.Cryptography;

namespace DayPlanner.Web.Components.Layout
{
    public partial class MainLayout
    {
        #region Injections
        [Inject]
        private ProtectedLocalStorage ProtectedLocalStore { get; set; } = default!;
        [Inject]
        private AuthenticationStateProvider StateProvider { get; set; } = default!;
        [Inject]
        private IStringLocalizer<MainLayout> Localizer { get; set; } = default!;
        [Inject]
        private TooltipService TooltipService { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        #endregion
        private bool sidebar1Expanded = true;
        private bool showCookieDialog = true;
        private bool showSettings;
        private bool cookieConsent = false;
        private bool analyticsCookies = false;
        private bool marketingCookies = false;
        private bool necessaryCookies = true; // Always active
        User? _user;

        private async void OnUpdateAuthenticationState(Task<AuthenticationState> stateTask)
        {
            AuthenticationState state = await stateTask;
            _user = state.User.Identity?.IsAuthenticated ?? false
                ? _user = state.User.ToUser()
                : null;
        }
        protected override void OnInitialized()
        {
            StateProvider.AuthenticationStateChanged += OnUpdateAuthenticationState;
            OnUpdateAuthenticationState(StateProvider.GetAuthenticationStateAsync());
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {


                    var consentResult = await ProtectedLocalStore.GetAsync<bool>("cookieConsent");
                    cookieConsent = consentResult.Success && consentResult.Value;

                    var analyticsResult = await ProtectedLocalStore.GetAsync<bool>("analyticsCookies");
                    analyticsCookies = analyticsResult.Success ? analyticsResult.Value : false;

                    var marketingResult = await ProtectedLocalStore.GetAsync<bool>("marketingCookies");
                    marketingCookies = marketingResult.Success ? marketingResult.Value : false;

                    // If we have any consent, ensure base consent is true
                    if (analyticsCookies || marketingCookies)
                    {
                        cookieConsent = true;
                    }
                }
                catch (CryptographicException)
                {
                    // Handle decryption errors (e.g., when key changes)
                    await ProtectedLocalStore.DeleteAsync("cookieConsent");
                    await ProtectedLocalStore.DeleteAsync("analyticsCookies");
                    await ProtectedLocalStore.DeleteAsync("marketingCookies");
                    cookieConsent = false;
                }
                catch
                {
                    cookieConsent = false;
                }

                showCookieDialog = !cookieConsent;
                StateHasChanged();
            }
        }

        private string GetUserAvatar()
        {
            if (_user is null || string.IsNullOrWhiteSpace(_user.DisplayName))
                return "U"; // Standard-Avatar, wenn kein Benutzername vorhanden ist.
            if (!string.IsNullOrEmpty(_user.PhotoUrl))
            {
                return _user.PhotoUrl;
            }
            // Generiere die Initialen aus dem Benutzernamen.
            var parts = _user.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = string.Concat(parts.Select(part => part[0]).Take(2)); // Nimm die ersten Buchstaben von maximal zwei Wörtern.
            return initials.ToUpper(); // Stelle sicher, dass die Initialen großgeschrieben sind.
        }
        private async Task HandleAcceptCookies(bool acceptAll = false)
        {
            try
            {
                cookieConsent = true;
                analyticsCookies = acceptAll ? true : analyticsCookies;
                marketingCookies = acceptAll ? true : marketingCookies;

                await SavePreferences();
                showCookieDialog = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving preferences: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task SavePreferences()
        {
            await ProtectedLocalStore.SetAsync("cookieConsent", cookieConsent);
            await ProtectedLocalStore.SetAsync("analyticsCookies", analyticsCookies);
            await ProtectedLocalStore.SetAsync("marketingCookies", marketingCookies);
        }
        private void Logout() => Navigation.NavigateTo("/account/logout",true);
    }
}
