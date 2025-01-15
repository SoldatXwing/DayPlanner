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

        builder.Services.AddRefitClient<IDayPlannerApi>(ConfigureDayPlannerRefit, httpClientName: "RefitClient.DayPlanner")
            .ConfigureHttpClient(client => builder.Configuration.Bind("DayPlannerApi:HttpClient", client));

        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddSingleton<AuthProvider>()
            .AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthProvider>());

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

    private static RefitSettings? ConfigureDayPlannerRefit(IServiceProvider provider)
    {
        AuthProvider authProvider = provider.GetRequiredService<AuthProvider>();
        return new()
        {
            AuthorizationHeaderValueGetter = async (_, _) =>
            {
                AuthenticationState state = await authProvider.GetAuthenticationStateAsync();
                if (state.User.Identity?.IsAuthenticated ?? false)
                {
                    _ = state.User.ToUser(out string authToken);
                    return authToken;
                }
                else
                {
                    return string.Empty;
                }
            }
        };
    }
}