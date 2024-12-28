using Carter;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Authorization.Services;
using DayPlanner.FireStore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DayPlanner.Api.Extensions
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder.Configuration);
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:TokenUri"]);
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:Audience"]);
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:ValidIssuer"]);

            //services.AddDbContext<DayPlannerDbContext>(options =>
            //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpClient<IJwtProvider, JwtProvider>((sp, httpClient) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                httpClient.BaseAddress = new Uri(configuration["Authentication:TokenUri"]!);
            });

            builder.Services.AddScoped<IAuthService>(provider => new AuthService("serviceAccountKey.json"));
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserStore>(provider => new FireStoreUserStore("serviceAccountKey.json"));
            builder.Services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
                {
                    jwtOptions.Authority = builder.Configuration["Authentication:ValidIssuer"];
                    jwtOptions.Audience = builder.Configuration["Authentication:Audience"];
                    jwtOptions.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:ValidIssuer"];
                });
            builder.Services.AddAuthorization();

            builder.Services.AddCarter();
        }
    }
}
