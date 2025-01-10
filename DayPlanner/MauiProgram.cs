using Blazored.LocalStorage;
using DayPlanner.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MudBlazor.Translations;

namespace DayPlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        builder.Services.AddMauiBlazorWebView();

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddScoped<AuthenticationStateProvider, LocalStorageAuthenticationProvider>();

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources/Localization");

        builder.Services
            .AddMudServices()
            .AddMudTranslations()
            .AddBlazoredLocalStorage();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}