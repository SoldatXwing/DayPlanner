using DayPlanner.BackgroundServices.Extensions;
using DayPlanner.BackgroundServices.Jobs;
using Quartz;

namespace DayPlanner.BackgroundServices
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")))
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
                Console.WriteLine("Environment set to Development");
            }
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

            hostBuilder.RegisterStores();
            hostBuilder.AddBackgroundServices();


            var host = hostBuilder.Build();
            await host.RunAsync();
        }
    }
}
