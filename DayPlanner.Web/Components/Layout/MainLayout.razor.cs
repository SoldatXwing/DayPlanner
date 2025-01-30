using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen.Blazor;
using System.Security.Cryptography;

namespace DayPlanner.Web.Components.Layout
{
    public partial class MainLayout
    {
        #region Injections
        [Inject]
        private ProtectedLocalStorage ProtectedLocalStore { get; set; } = default!;
        #endregion
        private bool sidebar1Expanded = true;
        private bool showCookieDialog = true;
        private bool showSettings;
        private bool cookieConsent = false;
        private bool analyticsCookies = false;
        private bool marketingCookies = false;
        private bool necessaryCookies = true; // Always active

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

        private async Task HandleSavePreferences()
        {
            try
            {
                cookieConsent = necessaryCookies || analyticsCookies || marketingCookies;
                await SavePreferences();
                showSettings = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving preferences: {ex.Message}");
            }
        }

        private async Task SavePreferences()
        {
            await ProtectedLocalStore.SetAsync("cookieConsent", cookieConsent);
            await ProtectedLocalStore.SetAsync("analyticsCookies", analyticsCookies);
            await ProtectedLocalStore.SetAsync("marketingCookies", marketingCookies);
        }
    }
}
