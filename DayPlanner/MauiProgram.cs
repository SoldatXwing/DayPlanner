using DayPlanner.Authentication;
using DayPlanner.Extensions;
using DayPlanner.Refit;
using DayPlanner.Services;
using DayPlanner.Services.Implementations;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MudBlazor.Translations;
using Refit;
using System.Resources;

[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.Satellite)]     // Sets the default fallback culture

namespace DayPlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddJsonConfiguration($"{typeof(MauiProgram).Assembly.GetName().Name}.appsettings.json")
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        builder.Services.AddMauiBlazorWebView();

        builder.AddRefitClients();
        builder.Services.AddMemoryCache();

        builder.Services
            .AddScoped<IAuthenticationService, DefaultAuthenticationService>()
            .AddSingleton<IPersistentStore, DefaultPersistentStore>();

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddSingleton<AuthenticationStateProvider, StoreAuthStateProvider>();

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources/Localization");

        builder.Services
            .AddMudServices()
            .AddMudTranslations();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder AddRefitClients(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddRefitClient<IDayPlannerAccountApi>(null, httpClientName: "RefitClient.DayPlannerAccount")
            .ConfigureHttpClient(client => builder.Configuration.Bind("DayPlannerApi:HttpClient", client));
        builder.Services
            .AddRefitClient<IDayPlannerApi>(settingsAction: sp =>
            {
                IPersistentStore store = sp.GetRequiredService<IPersistentStore>();
                return new()
                {
                    AuthorizationHeaderValueGetter = async (_, _) => (await store.GetUserAsync())?.accessToken ?? string.Empty
                };
            }, httpClientName: "RefitClient.DayPlanner")
            .ConfigureHttpClient(client => builder.Configuration.Bind("DayPlannerApi:HttpClient", client));

        return builder;
    }
}