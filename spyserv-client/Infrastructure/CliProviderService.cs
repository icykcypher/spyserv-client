using spyserv.Core;
using Newtonsoft.Json;
using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace spyserv.Infrastructure
{
    public static class CliProviderService 
    {
        private static readonly object FileLock = new object();
        public static void ConfigureCommands(string[] args)
        {
            var rootCommand = new RootCommand("SpyServ CLI - Manage and monitor your system and applications");

            var start = new Command("start", "Launch the system monitoring tools");
            start.SetHandler(StartServices);
            rootCommand.AddCommand(start);

            var stop = new Command("stop", "Stop the system monitoring tools");
            stop.SetHandler(StopServices);
            rootCommand.AddCommand(stop);

            var status = new Command("status", "Display the current status of the application");
            status.SetHandler(ShowStatus);
            rootCommand.AddCommand(status);

            // TRACK START
            var track = new Command("track", "Add the specified application to the monitoring list");

            var trackArgument = new Argument<string>("appName", "Name of the application to monitor");
            track.AddArgument(trackArgument);

            var trackLogs = new Option<string>(
                name: "--logs",
                getDefaultValue: () => string.Empty,
                description: "Specify the path to application logs"
            );
            trackLogs.AddAlias("-l");
            track.AddOption(trackLogs);

            var trackDescription = new Option<string>(
                name: "--description",
                getDefaultValue: () => string.Empty,
                description: "Specify the description of the application"
            );
            trackDescription.AddAlias("-d");
            track.AddOption(trackDescription);

            var trackRestart = new Option<bool>(
                name: "--restart",
                getDefaultValue: () => false,
                description: "Enable auto-restart if the application stops"
            );
            trackRestart.AddAlias("-r");
            var executablePath = new Option<string>(
                name: "--exec-path",
                description: "Path to the executable file",
                getDefaultValue: () => ""
            );
            executablePath.AddAlias("-e");

            trackRestart.AddValidator(result =>
            {
                if (result.GetValueOrDefault<bool>() && result.Parent.GetValueForOption<string>(executablePath) == "--exec-path") 
                {
                    result.ErrorMessage = "The --exec-path option is required when using --restart.";
                }
            });
            track.AddOption(trackRestart);
            track.AddOption(executablePath);

            var trackCheckingInterval = new Option<int>(
                name: "--checking-interval",
                getDefaultValue: () => 60,
                description: "Set the interval in seconds for checking the application status"
            );
            trackCheckingInterval.AddAlias("-ci");
            track.AddOption(trackCheckingInterval);

            var trackRestartDelay = new Option<int>(
                name: "--restart-delay",
                getDefaultValue: () => 0,
                description: "Set the delay in seconds before restarting the application"
            );
            trackRestartDelay.AddAlias("-rd");
            track.AddOption(trackRestartDelay);

            var trackNoNotify = new Option<bool>(
                name: "--no-notify",
                getDefaultValue: () => false,
                description: "Disable notifications for application"
            );
            trackNoNotify.AddAlias("-n");
            track.AddOption(trackNoNotify);

            track.SetHandler(async (appName, logs, description, restart, 
                        checkingInterval, restartDelay, noNotify, execPath) =>
            {
                var app = new MonitoredApp
                {
                    Name = appName,
                    PathToBin = execPath,
                    PathToLogs = logs,
                    Description = description,
                    IsRunning = true,
                    AutoRestart = restart,
                    CheckingIntervalInSec = checkingInterval,
                    RestartDelay = restartDelay,
                    NoNotify = noNotify
                };

                await TrackApplication(app);
                Console.WriteLine($"Tracking application: {app.Name}");
                Console.WriteLine($"Path to exectable file: {execPath}");
                Console.WriteLine($"Logs Path: {app.PathToLogs}");
                Console.WriteLine($"Description: {app.Description}");
                Console.WriteLine($"Auto Restart: {app.AutoRestart}");
                Console.WriteLine($"Checking Interval: {app.CheckingIntervalInSec} seconds");
                Console.WriteLine($"Restart Delay: {app.RestartDelay} seconds");
                Console.WriteLine($"No Notify: {app.NoNotify}");

                await Task.CompletedTask;
            },
            trackArgument,
            trackLogs,
            trackDescription,
            trackRestart,
            trackCheckingInterval,
            trackRestartDelay,
            trackNoNotify,
            executablePath
            );

            rootCommand.AddCommand(track);
            // TRACK END
            // UNTRACK START
            var untrack = new Command("untrack", "Remove the specified application from the monitoring list");
            var untrackArgument = new Argument<string>("appName", "Name of the application to untrack");
            untrack.AddArgument(untrackArgument);

            untrack.SetHandler
            (
                UntrackApplication,
                untrackArgument
            );
            rootCommand.AddCommand(untrack);
            // UNTRACK END
            // CONFIG START
            var config = new Command("config", "Set configuration values");
            var configCommandUserName = new Command("user.name", "Set user's name");
            var configCommandUserEmail = new Command("user.email", "Set user's email'");
            var configCommandApp = new Command("app", "Configure application settings");
            var configCommandResMon = new Command("resmon", "Configure resource monitoring settings");
            
            var configArgumentName = new Argument<string>("user-name", "Value for the specified user's name");
            configCommandUserName.AddArgument(configArgumentName);
            config.Add(configCommandUserName);
            
            var configArgumentEmail= new Argument<string>("user-email", "Value for the specified user's email");
            configCommandUserEmail.AddArgument(configArgumentEmail);
            configCommandUserName.SetHandler
            (
                ConfigureUserName,
                configArgumentName
            );

             configCommandUserEmail.SetHandler
            (
                ConfigureUserEmail,
                configArgumentEmail
            );
            config.Add(configCommandUserEmail);

            var checkApplicationsStatus = new Option<bool>(
                name: "--monitor-apps",
                description: "Enable or disable monitoring apps",
                getDefaultValue: () => true
            );
            checkApplicationsStatus.AddAlias("-ma");
            configCommandApp.AddOption(checkApplicationsStatus);

            var sendMonitoringData = new Option<bool>(
                name: "--send-data",
                getDefaultValue: () => true,
                description: "Send monitoring data on server"
            );
            sendMonitoringData.AddAlias("-sd");
            configCommandApp.AddOption(sendMonitoringData);

            var sendNotifications = new Option<bool>(
                name: "--send-notifications",
                getDefaultValue: () => true,
                description: "Send notifications"
            );
            sendNotifications.AddAlias("-sn");
            configCommandApp.AddOption(sendNotifications);

            var softExiting = new Option<bool>(
                name: "--soft-exit",
                getDefaultValue: () => true,
                description: "Alows to soft exit monitoring services"
            );
            softExiting.AddAlias("-se");
            configCommandApp.AddOption(softExiting);

            var monitoringIntervalForAll = new Option<int>(
                name: "--monitor-cycle",
                getDefaultValue: () => 60,
                description: "Monitoring interval for all applications"
            );
            monitoringIntervalForAll.AddAlias("-mc");
            configCommandApp.AddOption(monitoringIntervalForAll);

            var enableLogging = new Option<bool>(
                name: "--enable-logging",
                getDefaultValue: () => true,
                description: "Enable logging"
            );
            enableLogging.AddAlias("-el");
            configCommandApp.AddOption(enableLogging);
            
            configCommandApp.SetHandler((CheckApplicationsStatus, SendMonitoringData, SendNotifications, SoftExiting, 
                        EnableLogging, MonitoringInterval) =>
            {
                var AppSettings = new ServicesSettings
                {
                    CheckApplicationsStatus = CheckApplicationsStatus,
                    SendMonitoringData = SendMonitoringData,
                    SendNotifications = SendNotifications,
                    EnableLogging = EnableLogging,
                    MonitoringInterval = MonitoringInterval,
                    SoftExiting = SoftExiting
                };
                var config = GetConfig();
                config.AppSettings = AppSettings;
                SaveApplicationConfig(StaticClaims.PathToConfig, config);

                Console.WriteLine($"Check Applications Status: {CheckApplicationsStatus}");
                Console.WriteLine($"Send Monitoring Data: {SendMonitoringData}");
                Console.WriteLine($"Send Notifications: {SendNotifications}");
                Console.WriteLine($"Enable Logging: {EnableLogging}");
                Console.WriteLine($"Soft Exiting: {SoftExiting}");
                Console.WriteLine($"Monitoring Interval: {MonitoringInterval} seconds");
            },
            checkApplicationsStatus,
            sendMonitoringData,
            sendNotifications,
            softExiting,
            enableLogging,
            monitoringIntervalForAll);

            var monitorCpuUsage = new Option<bool>(
                name: "--monitor-cpu",
                getDefaultValue: () => true,
                description: "Monitor CPU usage for sending on server"
            );
            monitorCpuUsage.AddAlias("-mc");
            configCommandResMon.AddOption(monitorCpuUsage);

            var monitorMemsage = new Option<bool>(
                name: "--monitor-ram",
                getDefaultValue: () => true,
                description: "Monitor RAM usage for sending on server"
            );
            monitorMemsage.AddAlias("-mr");
            configCommandResMon.AddOption(monitorMemsage);

            var monitorDiskUsage = new Option<bool>(
                name: "--monitor-disk",
                getDefaultValue: () => true,
                description: "Monitor disk usage for sending on server"
            );
            monitorDiskUsage.AddAlias("-md");
            configCommandResMon.AddOption(monitorDiskUsage);

            var cpuUsageThreshold = new Option<int>(
                name: "--cpu-threshold",
                getDefaultValue: () => 80,
                description: "Set CPU usage threshold in %"
            );
            configCommandResMon.AddOption(cpuUsageThreshold);

            var ramUsageThreshold = new Option<int>(
                name: "--ram-threshold",
                getDefaultValue: () => 90,
                description: "Set RAM usage threshold in %"
            );
            configCommandResMon.AddOption(ramUsageThreshold);

            var disksageThreshold = new Option<int>(
                name: "--disk-threshold",
                getDefaultValue: () => 90,
                description: "Set disk usage threshold in %"
            );
            configCommandResMon.AddOption(disksageThreshold);

            configCommandResMon.SetHandler((monitorCpuUsage, monitorMemUsage, monitorDiskUsage, cpuUsageThreshold, 
                        ramUsageThreshold, disksageThreshold) =>
            {
                
                var config = GetConfig();
                config.ResMonSettings = new ResourceMonitoringSettings
                {
                    MonitorCpuUsage = monitorCpuUsage,
                    MonitorMemoryUsage = monitorMemUsage,
                    MonitorDiskUsage = monitorDiskUsage,
                    CpuUsageThreshold = cpuUsageThreshold,
                    MemoryUsageThreshold = ramUsageThreshold,
                    DiskUsageThreshold = disksageThreshold
                };

                SaveApplicationConfig(StaticClaims.PathToConfig, config);

                Console.WriteLine($"MonitorCpuUsage: {monitorCpuUsage}");
                Console.WriteLine($"MonitorMemoryUsage: {monitorMemUsage}");
                Console.WriteLine($"MonitorDiskUsage: {monitorDiskUsage}");
                Console.WriteLine($"CpuUsageThreshold: {cpuUsageThreshold}%");
                Console.WriteLine($"MemoryUsageThreshold: {ramUsageThreshold}%");
                Console.WriteLine($"DiskUsageThreshold: {disksageThreshold}%");
            },
            monitorCpuUsage,
            monitorMemsage,
            monitorDiskUsage,
            cpuUsageThreshold,
            ramUsageThreshold,
            disksageThreshold);

            config.Add(configCommandApp);
            config.Add(configCommandResMon);
            rootCommand.AddCommand(config);
            // CONFIG END
            rootCommand.Invoke(args);
        }
        /// <summary>
        /// Configures user information for email notifications and remote server control.
        /// </summary>
        /// <param name="args">
        /// Command-line arguments, where:
        /// args[1] specifies the configuration key (e.g., "user.name" or "user.email"),
        /// and args[2] specifies the corresponding value.
        /// </param>
        /// <remarks>
        /// This method validates input parameters and ensures the provided email address
        /// is in a correct format if "user.email" is being configured.
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when required arguments are missing.
        /// </exception>
        private static void ConfigureUserName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                Console.WriteLine("spyserv config: Incorrect parameter");
            else
            {
                if (File.Exists(StaticClaims.PathToConfig))
                {
                    var json = File.ReadAllText(StaticClaims.PathToConfig);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);
                    config.User.Name = value;
                    SaveApplicationConfig(StaticClaims.PathToConfig, config);
                }
                else 
                {
                    var config = CreateNewAppConfig();
                    config.User.Name = value;
                    SaveApplicationConfig(StaticClaims.PathToConfig, config);
                }
            }
        }

        private static void ConfigureUserEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                Console.WriteLine("spyserv config: Incorrect parameter");
            else
            {
                if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    Console.WriteLine("spyserv config: Incorrect email format");
                else 
                {
                    if (File.Exists(StaticClaims.PathToConfig))
                    {
                        var json = File.ReadAllText(StaticClaims.PathToConfig);
                        var config = JsonConvert.DeserializeObject<AppConfig>(json);
                        config.User.Email = value;
                        SaveApplicationConfig(StaticClaims.PathToConfig, config);
                    }
                    else 
                    {
                        var config = CreateNewAppConfig();
                        config.User.Email = value;
                        SaveApplicationConfig(StaticClaims.PathToConfig, config);
                    }
                }
            }
        }

        /// <summary>
        /// Stops spyserv services
        /// </summary>
        private static void StopServices()
        {
            try
            {
                var processes = Process.GetProcessesByName(StaticClaims.SpyservServiceProcessName);

                if (processes.Length > 0) foreach (var process in processes) process.Kill();
                else Console.WriteLine($"spyserv stop: Monitoring services was not working");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv stop: Error while trying to kill process: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds to config.json application name 
        /// </summary>
        /// <param name="appName">Application name</param>
        private async static Task TrackApplication(MonitoredApp app)
        {
            if (app is not null) AddAppToConfig(app);
            else Console.WriteLine("spyserv track: Invalid arguments for configuring.");

            await Task.CompletedTask;
        }

        private static void AddAppToConfig(MonitoredApp app)
        {
            var configFilePath = Path.Combine(AppContext.BaseDirectory, StaticClaims.PathToMonitoredAppsConf);
            MonitoredAppsConfig config;

            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<MonitoredAppsConfig>(json);
            }
            else config = CreateNewMonitoringConfig();

            if (!config.MonitoredApps.Contains(app)) config.MonitoredApps.Add(app);

            SaveMonitoringConfig(configFilePath, config);
        }

        private static AppConfig CreateNewAppConfig()
        {
            var config = new AppConfig();
            config.AppSettings = new ServicesSettings();
            config.ResMonSettings = new ResourceMonitoringSettings();
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
                Name = config.User?.Name ?? "null",
                Email = config.User?.Email ?? "null"
            };

            return config;
        }

        private static AppConfig GetConfig()
        {
            if (File.Exists(StaticClaims.PathToConfig))
            {
                var json = File.ReadAllText(StaticClaims.PathToConfig);
                return JsonConvert.DeserializeObject<AppConfig>(json) 
                ?? throw new JsonSerializationException($"Error in desirialization {StaticClaims.PathToConfig}.");
            }
            else
            {
                var conf = CreateNewAppConfig();
                SaveApplicationConfig(StaticClaims.PathToConfig, conf);
                return conf;
            }
        }

        private static MonitoredAppsConfig CreateNewMonitoringConfig()
        {
            var config = new MonitoredAppsConfig();

            return config;
        }

        private static MonitoredAppsConfig LoadMonitoringConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<MonitoredAppsConfig>(json) 
                ?? throw new JsonSerializationException($"Error in desirialization {configFilePath}.");
            }
            else return CreateNewMonitoringConfig();
        }

        private static void SaveMonitoringConfig(string configFilePath, MonitoredAppsConfig config)
        {
            if (File.Exists(configFilePath))
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
            }
            else 
            {
                Directory.CreateDirectory("../src/");
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
            }
        }

        private static void SaveApplicationConfig(string configFilePath, AppConfig config)
        {
            if (File.Exists(configFilePath))
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
            }
            else 
            {
                Directory.CreateDirectory("../src/");
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.Create(StaticClaims.PathToConfig).Dispose();
                File.WriteAllText(configFilePath, json);
            }
        }

        private static void StartServices()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var folderPath = Path.Combine(baseDirectory, @"../spyserv-services/spyserv-services");
            var fullPath = Path.GetFullPath(folderPath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"spyserv start: Monitoring services binary was not found at {fullPath}");
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv start: Failed to start monitoring services: {ex.Message}");
            }
        }

        private static bool IsProcessRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv: Error while checking process '{processName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows spyserv api and watcher status 
        /// </summary>
        private static void ShowStatus()
        {
            var isSpyservServicesRunning = IsProcessRunning(StaticClaims.SpyservServiceProcessName);

            if (!isSpyservServicesRunning) Console.WriteLine("spyserv status: SpyServ is not running.");
            else Console.WriteLine("spyserv status: SpyServ is running.");
        }


        /// <summary>
        /// Removing from config.json application
        /// </summary>
        /// <param name="appName">Application name</param>
        private static void UntrackApplication(string appName)
        {
            var configFilePath = Path.Combine(AppContext.BaseDirectory, StaticClaims.PathToMonitoredAppsConf);

            var config = LoadMonitoringConfig(configFilePath);

            if (config.MonitoredApps.Select(x => x.Name).Contains(appName))
            {
                var app = config.MonitoredApps.First(a => a.Name == appName);
                config.MonitoredApps.Remove(app);
                SaveMonitoringConfig(configFilePath, config);
                Console.WriteLine($"spyserv untrack: Application '{appName}' removed from the config.");
            }
            else
            {
                Console.WriteLine($"spyserv untrack: Application '{appName}' is not found in the config.");
            }
        }
    }
}