using DayPlanner.BackgroundServices.Extensions;
using DayPlanner.BackgroundServices.Jobs;
using Quartz;

namespace DayPlanner.BackgroundServices
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);

            hostBuilder.RegisterStores();
            hostBuilder.AddBackgroundServices();


            var host = hostBuilder.Build();
            await host.RunAsync();
        }
    }
}
