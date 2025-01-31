using DayPlanner.Web.Refit;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Refit;
using System;

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
                var storage = sp.GetRequiredService<ProtectedLocalStorage>();
                return new RefitSettings
                {
                    AuthorizationHeaderValueGetter = async (_, _) =>
                    {
                        var token = await storage.GetAsync<string>("authToken");
                        return token.Value ?? string.Empty;
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
