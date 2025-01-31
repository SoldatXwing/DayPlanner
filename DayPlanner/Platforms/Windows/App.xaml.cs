using DayPlanner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DayPlanner.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {      
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var appInstance = AppInstance.GetCurrent();
            var e = appInstance.GetActivatedEventArgs();

            // If it's not a Protocol activation, just launch the app normally
            if (e.Kind != ExtendedActivationKind.Protocol ||
                e.Data is not ProtocolActivatedEventArgs protocol)
            {
                appInstance.Activated += AppInstance_Activated;

                base.OnLaunched(args);
                return;
            }

            // If it's a Protocol activation, redirect it to other instances
            var instances = AppInstance.GetInstances();
            await Task.WhenAll(instances
                .Select(async q => await q.RedirectActivationToAsync(e)));

            return;
        }
        private void AppInstance_Activated(object? sender, AppActivationArguments e)
        {
            if (e.Kind != ExtendedActivationKind.Protocol ||
                e.Data is not ProtocolActivatedEventArgs protocol)
            {
                return;
            }
            _ = HandleGoogleLogin(protocol.Uri);


        }
        private static async Task HandleGoogleLogin(Uri uri)
        {
            var authenticationService = Current.Services.GetService<IAuthenticationService>();
            if (authenticationService == null)
            {
                Log.Warning("AuthenticationService is not available.");
                return;
            }
                
            try
            {
                var queryParams = uri.Query
                    .TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split('=', 2))
                    .ToDictionary(
                        arr => arr[0],
                        arr => arr.Length > 1 ? arr[1] : null
                    );

                if (!queryParams.TryGetValue("token", out var token) || string.IsNullOrEmpty(token))
                {
                    Log.Warning("Missing or empty 'token' parameter in URI: {Uri}", uri);
                    return;
                }

                if (!queryParams.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
                {
                    Log.Warning("Missing or empty 'refreshToken' parameter in URI: {Uri}", uri);
                    return;
                }

                await authenticationService.LoginViaGoogleAsync(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while handling Google login for URI: {Uri}", uri);
            }
        }
    }

}
