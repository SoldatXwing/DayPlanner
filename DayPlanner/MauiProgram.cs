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
using Serilog;
using System.Resources;

[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.Satellite)]     // Sets the default fallback culture

namespace DayPlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if ANDROID
        AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);     // Required otherwise app craches on android
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddDefaultJsonConfigs()
            .ConfigureLogging()
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
            .AddSingleton<StoreAuthStateProvider>()
            .AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<StoreAuthStateProvider>());

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources/Localization");

        builder.Services
            .AddMudServices()
            .AddMudTranslations();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder AddDefaultJsonConfigs(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        string assemblyName = typeof(MauiProgram).Assembly.GetName().Name!;
        return builder
            .AddJsonConfiguration($"{assemblyName}.appsettings.json", optional: false)
            .AddJsonConfiguration($"{assemblyName}.appsettings.{Environment.CurrentEnv}.json")
            .AddJsonConfiguration($"{assemblyName}.appsettings.{Environment.Platform}.json")
            .AddJsonConfiguration($"{assemblyName}.appsettings.{Environment.Platform}.{Environment.CurrentEnv}.json");
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

    private static MauiAppBuilder ConfigureLogging(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.ClearProviders();

        Serilog.Core.Logger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs", ".log"), rollingInterval: RollingInterval.Day)
#if DEBUG
            .WriteTo.Debug()
#endif
#if ANDROID
            .WriteTo.Sink(new AndroidEventSink())
#elif WINDOWS
            .WriteTo.EventLog("DayPlanner")
#endif
            .CreateLogger();
        builder.Logging.AddSerilog(logger);

        return builder;
    }
}