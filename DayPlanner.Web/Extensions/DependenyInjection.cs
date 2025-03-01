using DayPlanner.Web.Refit;
using Refit;

namespace DayPlanner.Web.Extensions;

internal static class DependencyInjection
{
    /// <summary>
    /// Registers all Refit API clients for the DayPlanner application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRefitClients(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register IDayPlannerAccountApi (No Authorization)
        services.AddRefitClient<IDayPlannerAccountApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new("https+http://dayplanner-api/v1");
                client.Timeout = TimeSpan.FromSeconds(60); //Increase Timeout for AI response
            });

        // Register IDayPlannerApi (With Authorization)
        services.AddRefitClient<IDayPlannerApi>(settingsAction: sp =>
        {
            return new RefitSettings
            {
                AuthorizationHeaderValueGetter = (_, _) =>
                {
                    HttpContext? context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

                    string tokenValue = context is not null && (context.User.Identity?.IsAuthenticated ?? false)
                        ? context.User.ToUserSession().Token
                        : string.Empty;
                    return Task.FromResult(tokenValue);
                }
            };
        })
            .ConfigureHttpClient(client => client.BaseAddress = new("https+http://dayplanner-api/v1"));

        return services;
    }
}
