using AutoMapper;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.BackgroundServices.Jobs;
using DayPlanner.FireStore;
using DayPlanner.ThirdPartyImports.Google_Calendar;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Quartz;

namespace DayPlanner.BackgroundServices.Extensions
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Add Quartz background services to the host.
        /// </summary>
        public static void AddBackgroundServices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddQuartz(q =>
                {
                    var jobKey = new JobKey("GoogleSyncJob");
                    q.AddJob<GoogleSyncJob>(opts => opts.WithIdentity(jobKey));

                    q.UseMicrosoftDependencyInjectionJobFactory();

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("GoogleSyncJob-trigger")
                        .WithSimpleSchedule(c => c.WithIntervalInHours(1).RepeatForever()));
                });

                services.AddQuartzHostedService(options =>
                {
                    options.WaitForJobsToComplete = true;
                });
            });
        }

        /// <summary>
        /// Register all stores and services required by the background services.
        /// </summary>
        public static void RegisterStores(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                var (firebaseApp, fireStoreDb) = InitializeFirebase(context);

                RegisterFirestoreStores(services, firebaseApp, fireStoreDb);

                services.AddAutoMapper(typeof(MappingProfile));

                RegisterGoogleServices(context, services, fireStoreDb);
            });
        }

        /// <summary>
        /// Initializes Firebase and Firestore.
        /// </summary>
        private static (FirebaseApp, FirestoreDb) InitializeFirebase(HostBuilderContext context)
        {
            string authFile = context.Configuration["FireBase:AuthFile"] ?? throw new InvalidOperationException("Firebase AuthFile is required.");
            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, authFile);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Firebase service account key file not found: {filePath}");

            var serviceAccountKeyJson = File.ReadAllText(filePath);
            string projectId = context.Configuration["FireBase:ProjectId"] ?? throw new InvalidOperationException("Firebase ProjectId is required.");

            var firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountKeyJson),
                ProjectId = projectId
            });

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
            var fireStoreDb = FirestoreDb.Create(projectId);

            return (firebaseApp, fireStoreDb);
        }

        /// <summary>
        /// Register all Firestore stores required by the background services.
        /// </summary>
        private static void RegisterFirestoreStores(IServiceCollection services, FirebaseApp firebaseApp, FirestoreDb fireStoreDb)
        {
            services.AddScoped<IUserStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreUserStore(firebaseApp, mapper);
            });

            services.AddScoped<IGoogleSyncTokenStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                var userStore = provider.GetRequiredService<IUserStore>();
                return new FireStoreGoogleSyncTokenStore(fireStoreDb, userStore, mapper);
            });

            services.AddScoped<IAppointmentStore>(provider =>
            {
                var mapper = provider.GetRequiredService<IMapper>();
                return new FireStoreAppointmentStore(fireStoreDb, mapper);
            });
        }

        /// <summary>
        /// Registriert services needed for googlecalendarservice
        /// </summary>
        private static void RegisterGoogleServices(HostBuilderContext context, IServiceCollection services, FirestoreDb fireStoreDb)
        {
            if (!context.Configuration.GetSection("GoogleConfig").Exists())            
                throw new NotImplementedException("Missing GoogleConfig section in appsettings");

            string googleCalendarTokenUri = context.Configuration["GoogleConfig:TokenUri"] ?? throw new("Google token URI is required.");
            string googleClientId = context.Configuration["google_client_id"] ?? throw new("Google client ID is required. Configured in user-secrets, key: google_client_id");
            string googleClientSecret = context.Configuration["google_client_secret"] ?? throw new("Google client secret is required. Configured in user-secrets, key: google_client_secret"); //From User Secrets

            services.AddHttpClient();

            services.AddScoped<IGoogleRefreshTokenStore>(provider =>
                new FireStoreGoogeRefreshTokenStore(fireStoreDb, provider.GetRequiredService<IUserStore>()));

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
        }
    }
}
