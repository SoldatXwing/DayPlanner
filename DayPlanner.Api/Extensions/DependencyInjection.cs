using AutoMapper;
using Carter;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Authorization.Services;
using DayPlanner.FireStore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DayPlanner.Api.Extensions
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:TokenUri"]);
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:Audience"]);
            ArgumentNullException.ThrowIfNull(builder.Configuration["Authentication:ValidIssuer"]);

            string authFile = builder.Configuration["FireBase:AuthFile"] ?? throw new InvalidOperationException("An authentication file for firebase is required. Config path: 'FireBase:AuthFile'.");
            string projectId = builder.Configuration["FireBase:ProjectId"] ?? throw new InvalidOperationException("The firebase project id id required. Config path: 'FireBase:ProjectId'.");

            FirebaseApp firebaseApp = FirebaseApp.Create(new AppOptions
            {
                ProjectId = projectId,
                Credential = GoogleCredential.FromFile(authFile),
            });
            builder.Services.AddScoped<IAuthService>(provider => new AuthService(firebaseApp));
            builder.Services.AddScoped<IUserStore>(provider => new FireStoreUserStore(firebaseApp));

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", authFile);
            FirestoreDb firestoreDb = FirestoreDb.Create(projectId);
            builder.Services.AddScoped<IAppointmentStore>(provider => new FireStoreAppointmentStore(firestoreDb, provider.GetRequiredService<IMapper>()));

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAppointmentsService, AppointmentsService>();

            builder.Services.AddHttpClient<IJwtProvider, JwtProvider>((sp, httpClient) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                httpClient.BaseAddress = new Uri(configuration["Authentication:TokenUri"]!);
            });

            builder.Services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
                {
                    jwtOptions.Authority = builder.Configuration["Authentication:ValidIssuer"];
                    jwtOptions.Audience = builder.Configuration["Authentication:Audience"];
                    jwtOptions.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:ValidIssuer"];
                });
            builder.Services.AddAuthorization();

            builder.Services.AddCarter();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
        }
    }
}
