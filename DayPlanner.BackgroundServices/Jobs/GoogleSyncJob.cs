using DayPlanner.Abstractions.Stores;
using DayPlanner.ThirdPartyImports.Google_Calendar;
using Quartz;

namespace DayPlanner.BackgroundServices.Jobs
{
    /// <summary>
    /// A job that synchronizes all Google appointments for all users, that are connected with google calendar.
    /// </summary>
    /// <param name="syncTokenStore">Store for sync tokens</param>
    /// <param name="logger">Logger</param>
    /// <param name="googleCalendarService">Service to interact with google calendar</param>
    public class GoogleSyncJob(
        IGoogleSyncTokenStore syncTokenStore,
        ILogger<GoogleSyncJob> logger,
        GoogleCalendarService googleCalendarService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var tokens = await syncTokenStore.GetAll();
            if (!tokens.Any())
            {
                logger.LogInformation("No sync tokens found. Skipping sync.");
                return;
            }
            foreach(var token in tokens!)
                await googleCalendarService.SyncAppointments(token.UserId);
            
            logger.LogInformation("Successfully synchronized all Google appointments. Synced for {tokenCount} accounts", tokens.Count());
        }
    }
}
