using AutoMapper;
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
    /// <summary>
    /// Class to add services to the dependency injection container.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the infrastucture to the webapplication builder.
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        /// <exception cref="InvalidOperationException">Throws if a appsettings required value is not provided</exception>
        /// <exception cref="FileNotFoundException">Throws if the service-account-key file isnt found</exception>
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

            var (firebaseApp, firestoreDb) = InitializeFirebase(builder.Configuration);

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
            builder.Services.AddScoped<GoogleOAuthService>();
        }

        private static void AddCustomServices(IServiceCollection services, FirebaseApp app, FirestoreDb db)
        {
            // Add strongly-typed configuration options
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            string googleCalendarTokenUri = configuration!["GoogleConfig:TokenUri"] ?? throw new("Google token URI is required.");
            string googleClientId = configuration["GoogleConfig:client_Id"] ?? throw new("Google client ID is required.");
            string googleClientSecret = configuration["GoogleConfig:client_Secret"] ?? throw new("Google client secret is required.");

            string authTokenUri = configuration!["Authentication:TokenUri"] ?? throw new("Authentication token URI is required.");
            string refreshTokenUri = configuration["Authentication:RefreshTokenUri"] ?? throw new("Refresh token URI is required.");
            string idpUri = configuration["Authentication:IdpUri"] ?? throw new("Idp URI is required.");

            // HttpClient for Authentication Token
            services.AddHttpClient("AuthTokenClient", client =>
            {
                client.BaseAddress = new Uri(authTokenUri);
            });

            // HttpClient for Refresh Token
            services.AddHttpClient("RefreshTokenClient", client =>
            {
                client.BaseAddress = new Uri(refreshTokenUri);
            });
            // HttpClient for Idp
            services.AddHttpClient("IdpClient", client =>
            {
                client.BaseAddress = new Uri(idpUri);
            });
            services.AddScoped<IJwtProvider, JwtProvider>();

            // Google-related services
            services.AddScoped<IGoogleTokenProvider>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var googleRefreshTokenStore = sp.GetRequiredService<IGoogleRefreshTokenStore>();
                httpClient.BaseAddress = new Uri(googleCalendarTokenUri);
                return new GoogleTokenProvider(
                    googleRefreshTokenStore,
                    httpClient,
                    googleClientId,
                    googleClientSecret
                );
            });

            services.AddScoped<GoogleCalendarService>();
            services.AddScoped<IGoogleRefreshTokenStore>(provider => new FireStoreGoogeRefreshTokenStore(db, provider.GetRequiredService<IUserStore>()));

            // Firebase-related services
            services.AddScoped<IAuthService>(provider => new AuthService(app));
            services.AddScoped<IUserStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreUserStore(app, mapper);
            });
            services.AddScoped<IAppointmentStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreAppointmentStore(db, mapper);
            });
            services.AddScoped<IGoogleSyncTokenStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                var userStore = provider.GetRequiredService<IUserStore>();
                return new FireStoreGoogleSyncTokenStore(db, userStore, mapper);
            });

        }

        /// <summary>
        /// Initializes Firebase and Firestore.
        /// </summary>
        private static (FirebaseApp, FirestoreDb) InitializeFirebase(IConfiguration config)
        {
            string authFile = config["FireBase:AuthFile"] ?? throw new InvalidOperationException("Firebase AuthFile is required.");
            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, authFile);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Firebase service account key file not found: {filePath}");

            var serviceAccountKeyJson = File.ReadAllText(filePath);
            string projectId = config["FireBase:ProjectId"] ?? throw new InvalidOperationException("Firebase ProjectId is required.");

            var firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountKeyJson),
                ProjectId = projectId
            });

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
            var fireStoreDb = FirestoreDb.Create(projectId);

            return (firebaseApp, fireStoreDb);
        }

    }
}
