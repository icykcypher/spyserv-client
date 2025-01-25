using Serilog;
using spyserv.Core;
using Newtonsoft.Json;
using System.Diagnostics;
using spyserv_services.Core.Dtos;

namespace spyserv_services.Services
{
    public class MonitoringService
    {
        private readonly Timer _checkingTimer;
        private List<MonitoredApp> _monitoredApps;
        private readonly Dictionary<string, DateTime> _lastCheckTimes;
        private readonly CommunicationService _communicationService;

        public MonitoringService(CommunicationService communicationService)
        {
            _monitoredApps = GetMonitoredApps();
            _lastCheckTimes = _monitoredApps.ToDictionary(app => app.Name, _ => DateTime.MinValue);

            _checkingTimer = new Timer(MonitorApps, null, 0, 1000);
            _communicationService = communicationService;
        }

        public void MonitorApps(object? state)
        {
            _monitoredApps = GetMonitoredApps();
            Log.Information("Started monitoring");
            foreach (var app in _monitoredApps)
            {
                if ((DateTime.Now - _lastCheckTimes[app.Name]).TotalSeconds >= app.CheckingIntervalInSec)
                {
                     CheckApplicationStatus(app);
                    _lastCheckTimes[app.Name] = DateTime.Now;
                }
            }

            Task.Run(() => _communicationService.SendMonitoringData(GetResourceUsage()));
        }

        private List<MonitoredApp> GetMonitoredApps()
        {
            var config = LoadConfig(StaticClaims.PathToConfig);
            return config.MonitoredApps;
        }

        private async Task CheckApplicationStatus(MonitoredApp app)
        {
            Log.Information($"Checking {app.Name}");
            if (!IsAppRunning(app.Name))
            {
                app.IsRunning = false;
                
                if (app.AutoRestart)
                {
                    await Task.Run(() => RestartApplication(app));
                }
                if (!app.NoNotify)
                {
                    await Task.Run(() => _communicationService.NotifyNotWorkingApp(app));
                }
            }
            else
            {
                app.IsRunning = true;
            }
        }

        private void RestartApplication(MonitoredApp app)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(app.Name))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = app.Name,
                        UseShellExecute = false 
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

        private bool IsAppRunning(string appName) =>  Process.GetProcessesByName(appName).Any();

        private MonitoringData GetResourceUsage()
        {
            try
            {
                var cpuUsage = ResourceMonitorService.GetCpuUsagePercentage();

                var memoryUsage = ResourceMonitorService.GetMemoryUsage();

                var diskUsage = ResourceMonitorService.GetDiskUsage();

                return new MonitoringData { CpuResult = cpuUsage, MemoryResult = memoryUsage, DiskResult = diskUsage };
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving system resource usage: {ex.Message}");
            }
            Log.Error("Error retrieving system resource usage");
            throw new Exception("Error retrieving system resource usage");
        }

        private Config LoadConfig(string configFilePath)
        {
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, StaticClaims.PathToConfig)))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
            }
            else return CreateNewConfig();
        }

        private static Config CreateNewConfig()
        {
            var config = new Config();
            config.Debug ??= new DebugConfig();
            config.Release ??= new ReleaseConfig();
            config.Debug.Pathes ??= new Pathes
            {
                SpyservApi = config.Debug?.Pathes?.SpyservApi ?? "../",
                SpyservWatcher = config.Debug?.Pathes?.SpyservWatcher ?? "../"
            };
            config.Release.Pathes ??= new Pathes
            {
                SpyservApi = config.Release?.Pathes?.SpyservApi ?? "../",
                SpyservWatcher = config.Release?.Pathes?.SpyservWatcher ?? "../"
            };
            config.User ??= new User
            {
                Name = config.User?.Name ?? "unknown",
                Email = config.User?.Email ?? "uknown"
            };
            
            return config;
        }
    }
}