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
using Newtonsoft.Json.Linq;

namespace DayPlanner.Api.Extensions
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this WebApplicationBuilder builder)
        {
            ValidateConfiguration(builder.Configuration,
            [
                "Authentication:TokenUri",
                "Authentication:Audience",
                "Authentication:ValidIssuer"
            ]);
            string basePath = AppContext.BaseDirectory; 
            string filePath = Path.Combine(basePath, "FireBase/serviceAccountKey.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Service account key file not found: {filePath}");
            

            var serviceAccountKeyjson = File.ReadAllText(filePath);
            var projectId = JObject.Parse(serviceAccountKeyjson)["project_id"]?.ToString();
            if (string.IsNullOrEmpty(projectId))
                throw new InvalidOperationException("Project ID is not provided in the service account key file.");

            var firebaseApp = InitializeFirebaseApp(serviceAccountKeyjson, projectId);
            var firestoreDb = InitializeFirestoreDb(filePath, projectId);

            AddAuthenticationServices(builder);
            AddCustomServices(builder.Services, projectId, firebaseApp, firestoreDb);

            builder.Services.AddCarter();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
        }

        private static void ValidateConfiguration(IConfiguration configuration, string[] requiredKeys)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            foreach (var key in requiredKeys)
            {
                if (string.IsNullOrEmpty(configuration[key]))
                {
                    throw new ArgumentNullException(key, $"Configuration value for '{key}' is missing.");
                }
            }
        }

        private static void AddAuthenticationServices(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.Authority = builder.Configuration["Authentication:ValidIssuer"];
                    jwtOptions.Audience = builder.Configuration["Authentication:Audience"];
                    jwtOptions.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:ValidIssuer"];
                });

            builder.Services.AddAuthorization();
        }

        private static void AddCustomServices(IServiceCollection services, string projectId, FirebaseApp app, FirestoreDb db)
        {
            services.AddHttpClient<IJwtProvider, JwtProvider>((sp, httpClient) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                httpClient.BaseAddress = new Uri(configuration["Authentication:TokenUri"]!);
            });

            services.AddScoped<IAuthService>(provider => new AuthService(app));
            services.AddScoped<IAppointmentsService, AppointmentsService>();
            services.AddScoped<IAppointmentStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreAppointmentStore(db, mapper);
            });
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserStore>(provider => new FireStoreUserStore(app));
        }
        private static FirebaseApp InitializeFirebaseApp(string serviceAccountKeyJson, string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Project ID in service account key file is missing.");
            }

            var app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountKeyJson),
                ProjectId = projectId
            });

            return app;
        }
        private static FirestoreDb InitializeFirestoreDb(string serviceAccountKeyPath, string projectId)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountKeyPath);
            return FirestoreDb.Create(projectId);
        }

    }
}
