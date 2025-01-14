using Serilog;
using Newtonsoft.Json;
using System.Diagnostics;
using spyserv_services.Core.Dtos;
using spyserv_c_api.Services;

namespace spyserv_services.Services
{
    public class MonitoringService
    {
        public void StartMonitoring()
        {
            while (true)
            {
                var apps = GetAppsToMonitor();
                foreach (var app in apps)
                {
                    CheckApplicationStatus(app);
                }

                LogResourceUsage();

                Thread.Sleep(60000);
            }
        }

        private static List<MonitoredApp> GetAppsToMonitor()
        {
            var config = LoadConfig(@"../../share/config.json");
            return config.AppsToMonitor;
        }

        private static void CheckApplicationStatus(MonitoredApp app)
        {
            var processes = Process.GetProcessesByName(app.Name);
            var isRunning = processes.Length > 0;

            if (!isRunning)
            {
                Log.Error($"Application '{app.Name}' has stopped working!");

                if (app.AutoRestart) RestartApplication(app);
            }
            else
            {
                Log.Information($"Application '{app.Name}' is running.");
            }

            app.IsRunning = isRunning;
        }

        private static void RestartApplication(MonitoredApp app)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(app.PathToLogs))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = app.PathToLogs,
                        UseShellExecute = true
                    });
                    Log.Information($"Application '{app.Name}' restarted successfully.");
                }
                else
                {
                    Log.Error($"Cannot restart application '{app.Name}' - no path specified.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to restart application '{app.Name}': {ex.Message}");
            }
        }

        private static void LogResourceUsage()
        {
            try
            {
                var cpuUsage = ResourceMonitorService.GetCpuUsagePercentage();
                Log.Information($"CPU Usage: {cpuUsage.UsagePercent}%");

                var memoryUsage = ResourceMonitorService.GetMemoryUsage();
                Log.Information($"Memory Usage: {memoryUsage.TotalMemoryMb * memoryUsage.UsedPercent / 100}MB / {memoryUsage.TotalMemoryMb}MB");

                var diskUsage = ResourceMonitorService.GetDiskUsage();
                Log.Information($"Disk Usage: Read {diskUsage.ReadMbps}Mbps, Write {diskUsage.WriteMbps}Mbps");
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving system resource usage: {ex.Message}");
            }
        }

        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
            }
            else return new Config { AppsToMonitor = new List<MonitoredApp>() };
        }
    }
}