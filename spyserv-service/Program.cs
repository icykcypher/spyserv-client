using Serilog;
using spyserv_service.Services;

namespace spyserv_service
{
    public class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, @"../logs/spyserv-watch.log"),
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("App started. Current Directory: {Directory}", AppContext.BaseDirectory);
            var monitoringService = new MonitoringService();

            monitoringService.StartMonitoring();
        }
    }
}