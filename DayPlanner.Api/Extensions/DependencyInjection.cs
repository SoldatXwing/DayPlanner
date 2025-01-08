﻿using AutoMapper;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Authorization.Services;
using DayPlanner.FireStore;
using DayPlanner.ThirdPartyImports.Google_Calendar;
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
            ValidateConfiguration(builder.Configuration,
            [
                "Authentication:TokenUri",
                "Authentication:Audience",
                "Authentication:ValidIssuer"
            ]);
            string authFile = builder.Configuration["FireBase:AuthFile"] ?? throw new InvalidOperationException("An authentication file for firebase is required. Config path: 'FireBase:AuthFile'.");
            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, authFile);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Service account key file not found: {filePath}");


            var serviceAccountKeyjson = File.ReadAllText(filePath);
            string projectId = builder.Configuration["FireBase:ProjectId"] ?? throw new InvalidOperationException("The firebase project id id required. Config path: 'FireBase:ProjectId'.");

            var firebaseApp = InitializeFirebaseApp(serviceAccountKeyjson, projectId);
            var firestoreDb = InitializeFirestoreDb(filePath, projectId);

            AddAuthenticationServices(builder);
            AddCustomServices(builder.Services, firebaseApp, firestoreDb);

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

        private static void AddCustomServices(IServiceCollection services, FirebaseApp app, FirestoreDb db)
        {
            // Add strongly-typed configuration options
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            string googleCalendarTokenUri = configuration!["GoogleCalendar:TokenUri"] ?? throw new("Google token URI is required.");
            string googleClientId = configuration["GoogleCalendar:client_Id"] ?? throw new("Google client ID is required.");
            string googleClientSecret = configuration["GoogleCalendar:client_Secret"] ?? throw new("Google client secret is required.");
            string authTokenUri = configuration["Authentication:TokenUri"] ?? throw new("Authentication token URI is required.");

            services.AddHttpClient<IJwtProvider, JwtProvider>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(authTokenUri);
            });

            // Google-related services
            services.AddScoped<IGoogleTokenProvider>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var googleRefreshTokenService = sp.GetRequiredService<IGoogleTokenService>();
                httpClient.BaseAddress = new Uri(googleCalendarTokenUri);
                return new GoogleTokenProvider(
                    googleRefreshTokenService,
                    httpClient,
                    googleClientId,
                    googleClientSecret
                );
            });

            services.AddScoped<GoogleCalendarService>();
            services.AddScoped<IGoogleTokenService, GoogleRefreshTokenService>();
            services.AddScoped<IGoogleRefreshTokenStore>(provider => new FireStoreGoogeRefreshTokenStore(db, app));

            // Firebase-related services
            services.AddScoped<IAuthService>(provider => new AuthService(app));
            services.AddScoped<IUserStore>(provider => new FireStoreUserStore(app));
            services.AddScoped<IAppointmentStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreAppointmentStore(db, mapper);
            });
            services.AddScoped<IGoogleSyncTokenStore>(provider => new FireStoreGoogleSyncTokenStore(db, app));

            // Application-specific services
            services.AddScoped<IAppointmentsService, AppointmentsService>();
            services.AddScoped<IUserService, UserService>();
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
