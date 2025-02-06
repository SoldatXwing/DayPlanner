using DayPlanner.Web.Refit;
using Refit;

namespace DayPlanner.Web.Extensions
{
    internal static class DependencyInjection
    {
        /// <summary>
        /// Registers all Refit API clients for the DayPlanner application.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddRefitClients(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Register IDayPlannerAccountApi (No Authorization)
            services.AddRefitClient<IDayPlannerAccountApi>()
                .ConfigureHttpClient(client =>
                {
                    var apiConfig = configuration.GetSection("DayPlannerApi:HttpClient");
                    client.BaseAddress = new Uri(apiConfig["BaseAddress"] ?? throw new InvalidOperationException("API Base Address not configured"));
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
            .ConfigureHttpClient(client =>
            {
                var apiConfig = configuration.GetSection("DayPlannerApi:HttpClient");
                client.BaseAddress = new Uri(apiConfig["BaseAddress"] ?? throw new InvalidOperationException("API Base Address not configured"));
            });

            return services;
        }
    }
}
