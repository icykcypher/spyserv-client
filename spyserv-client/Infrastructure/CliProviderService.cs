using spyserv.Core;
using Newtonsoft.Json;
using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace spyserv.Infastructure
{
    public static class CliProviderService 
    {
        public static void ConfigureCommands(string[] args)
        {
            var rootCommand = new RootCommand("SpyServ CLI - Manage and monitor your system and applications");

            var start = new Command("start", "Launch the system monitoring tools");
            start.SetHandler(StartServices);
            rootCommand.AddCommand(start);

            var stop = new Command("stop", "Stop all running services");
            stop.SetHandler(StopServices);
            rootCommand.AddCommand(stop);

            var status = new Command("status", "Display the current status of the application");
            status.SetHandler(ShowStatus);
            rootCommand.AddCommand(status);

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
            track.AddOption(trackRestart);

            var trackCheckingInterval = new Option<int>(
                name: "--checking-interval",
                getDefaultValue: () => 60,
                description: "Set the interval in seconds for checking the application status"
            );
            trackCheckingInterval.AddAlias("-i");
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
                description: "Disable notifications"
            );
            trackNoNotify.AddAlias("-n");
            track.AddOption(trackNoNotify);

            track.SetHandler(async (string appName, string logs, string description, bool restart, 
            int checkingInterval, int restartDelay, bool noNotify) =>
            {
                var app = new MonitoredApp
                {
                    Name = appName,
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
            trackNoNotify
            );

            rootCommand.AddCommand(track);

            var untrack = new Command("untrack", "Remove the specified application from the monitoring list");
            var untrackArgument = new Argument<string>("appName", "Name of the application to untrack");
            untrack.AddArgument(untrackArgument);

            untrack.SetHandler
            (
                UntrackApplication,
                untrackArgument
            );
            rootCommand.AddCommand(untrack);

            var config = new Command("config", "Set user configuration values");
            var configCommandUserName = new Command("user.name", "Set user's name");
            var configCommandUserEmail = new Command("user.email", "Set user's email'");

            config.Add(configCommandUserEmail);
            config.Add(configCommandUserName);
            
            var configArgumentName = new Argument<string>("user-name", "Value for the specified user's name");
            configCommandUserName.AddArgument(configArgumentName);

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
            rootCommand.AddCommand(config);

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
                    var config = JsonConvert.DeserializeObject<Config>(json) ?? CreateNewConfig();
                    config.User.Name = value;
                    SaveConfig(StaticClaims.PathToConfig, config);
                }
                else 
                {
                    var config = CreateNewConfig();
                    config.User.Name = value;
                    SaveConfig(StaticClaims.PathToConfig, config);
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
                        var config = JsonConvert.DeserializeObject<Config>(json) ?? CreateNewConfig();
                        config.User.Email = value;
                        SaveConfig(StaticClaims.PathToConfig, config);
                    }
                    else 
                    {
                         var config = CreateNewConfig();
                        config.User.Email = value;
                        SaveConfig(StaticClaims.PathToConfig, config);
                    }
                }
            }
        }

        /// <summary>
        /// Stops spyserv watcher
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
            var configFilePath = Path.Combine(AppContext.BaseDirectory, StaticClaims.PathToConfig);
            Config config;

            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<Config>(json) ?? CreateNewConfig();
            }
            else config = CreateNewConfig();

            if (!config.MonitoredApps.Contains(app)) config.MonitoredApps.Add(app);

            SaveConfig(configFilePath, config);
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

        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json) 
                ?? throw new JsonSerializationException($"Error in desirialization {configFilePath}.");
            }
            else
            {
                return new Config();
            }
        }

        private static void SaveConfig(string configFilePath, Config config)
        {
            var json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFilePath, json);
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
            var configFilePath = Path.Combine(AppContext.BaseDirectory, StaticClaims.PathToConfig);

            var config = LoadConfig(configFilePath);

            if (config.MonitoredApps.Select(x => x.Name).Contains(appName))
            {
                var app = config.MonitoredApps.First(a => a.Name == appName);
                config.MonitoredApps.Remove(app);
                SaveConfig(configFilePath, config);
                Console.WriteLine($"spyserv untrack: Application '{appName}' removed from the config.");
            }
            else
            {
                Console.WriteLine($"spyserv untrack: Application '{appName}' is not found in the config.");
            }
        }
    }
}