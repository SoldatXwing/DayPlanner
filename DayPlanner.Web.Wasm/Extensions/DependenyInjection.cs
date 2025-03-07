using Blazored.LocalStorage;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Models;
using DayPlanner.Web.Wasm.Refit;
using Microsoft.AspNetCore.Http;
using Refit;
using System.Text.Json.Serialization;

namespace DayPlanner.Web.Wasm.Extensions;

internal static class DependencyInjection
{
    /// <summary>
    /// Registers all Refit API clients for the DayPlanner application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRefitClients(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var serializer = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        serializer.Converters.Remove(serializer.Converters.Single(x => x.GetType().Equals(typeof(JsonStringEnumConverter))));

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
            var serviceProvider = sp.GetRequiredService<IServiceProvider>();
            return new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(serializer),
                AuthorizationHeaderValueGetter = async (_, ct) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var localStorage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
                    var userSession = await localStorage.GetItemAsync<UserSession>("userSession", ct);
                    return userSession?.Token ?? string.Empty;
                }
            };
        })
         .ConfigureHttpClient(client =>
        {
            var apiConfig = configuration.GetSection("DayPlannerApi:HttpClient");
            client.BaseAddress = new Uri(apiConfig["BaseAddress"] ?? throw new InvalidOperationException("API Base Address not configured"));
            client.Timeout = TimeSpan.FromSeconds(75); // Increase Timeout for AI response
        });

        return services;
    }
}
