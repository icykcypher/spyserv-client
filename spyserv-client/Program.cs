using spyserv_client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace spyserv
{
    /// <summary>
    /// Class representing assembly application in CLI
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args is null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                {
                    Console.WriteLine("spyserv: Unknown command. Use 'help' to see available commands.");
                    return;
                }

                var commandHandlers = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "start", _ => StartServices() },
                    { "status", _ => ShowStatus() },
                    { "help", _ => ShowCommands() },
                    { "stop", _ => StopServices() },
                    { "track", _ => TrackApplication(_[1]) },
                    { "untrack", _ => UntrackApplication(_[1]) },
                    { "config", ConfigureUser}
                };

                if (commandHandlers.TryGetValue(args[0], out var handler))
                {
                    handler(args);
                }
                else
                {
                    Console.WriteLine($"spyserv: Unknown command: {args[0]}. Use 'help' to see available commands.");
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"spyserv {args[1]}: Missing required parameters.");
            }
            catch(ArgumentNullException)
            {
                Console.WriteLine("spyserv: Unknown command. Use 'help' to see available commands");
            }
            catch (Exception e)
            {
                Console.WriteLine($"spyserv: An error occurred: {e.Message}");
            }
        }

        private static void StartServices()
        {
            StartApiAsDetachedProcess();
            StartWatcherAsDetachedProcess();
        }

        private static void StopServices()
        {
            StopApi();
            StopWatcher();
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
        private static void ConfigureUser(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("spyserv config: Missing parameter");
                    return;
                }

                if (args[1] == "user.name")
                {
                    if (string.IsNullOrWhiteSpace(args[2]))
                        Console.WriteLine("spyserv config: Incorrect parameter");
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (args[1] == "user.email")
                {
                    if (string.IsNullOrWhiteSpace(args[2]))
                        Console.WriteLine("spyserv config: Incorrect parameter");
                    else
                    {
                        if (!Regex.IsMatch(args[2], @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            Console.WriteLine("spyserv config: Incorrect email format");
                        else 
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                else Console.WriteLine("spyserv config: Unknown key");
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("spyserv config: Missing parameter");
            }
        }

        /// <summary>
        /// Starts the spyserv WebAPI for resource monitoring as a detached process.
        /// </summary>
        /// <remarks>
        /// This method locates the WebAPI binary, sets up the process start configuration, 
        /// and attempts to launch it in a minimized, detached state. If an error occurs, 
        /// it logs the failure to the console.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when the process fails to start, with the error message displayed in the console.
        /// </exception>
        private static void StartApiAsDetachedProcess()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var folderPath = Path.Combine(baseDirectory, @"../../../../spyserv-c-api/bin/Release/net8.0/spyserv-c-api");
            var fullPath = Path.GetFullPath(folderPath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"spyserv start: API binary was not found at {fullPath}");
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
                Console.WriteLine($"spyserv start: Failed to start API: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops spyserv api 
        /// </summary>
        private static void StopApi()
        {
            try
            {
                var processes = Process.GetProcessesByName(StaticClaims.ApiProcessName);

                if (processes.Length > 0) foreach (var process in processes) process.Kill();
                else Console.WriteLine($"spyserv stop: API was not working");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv stop: Error while trying to kill process: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops spyserv watcher
        /// </summary>
        private static void StopWatcher()
        {
            try
            {
                var processes = Process.GetProcessesByName(StaticClaims.WatcherProcessName);

                if (processes.Length > 0) foreach (var process in processes) process.Kill();
                else Console.WriteLine($"spyserv stop: watcher was not working");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv stop: Error while trying to kill process: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows available commands for assembly 
        /// </summary>
        private static void ShowCommands()
        {
            Console.WriteLine("spyserv: Available commands:");
            Console.WriteLine("\tstart                     - Launch the API server and system monitoring tools");
            Console.WriteLine("\tstatus                    - Display the current status of the application");
            Console.WriteLine("\thelp                      - List all available commands with descriptions");
            Console.WriteLine("\ttrack [app-name]          - Add the specified application to the monitoring list");
            Console.WriteLine("\tuntrack [app-name]        - Remove the specified application from the monitoring list");
            Console.WriteLine("\tconfig user.name [value]  - Set the user name for remote access configuration");
            Console.WriteLine("\tconfig user.email [value] - Set the user email for remote access configuration");
            Console.WriteLine("\tstop                      - Stop all running services and applications");
        }

        /// <summary>
        /// Adds to config.json application name 
        /// </summary>
        /// <param name="appName">Application name</param>
        private static void TrackApplication(string appName)
        {
            if (!String.IsNullOrWhiteSpace(appName)) AddAppToConfig(appName);
            else Console.WriteLine("spyserv track: Invalid arguments for configure.");
        }

        private static void AddAppToConfig(string appName)
        {
            string configFilePath = Path.Combine(AppContext.BaseDirectory, @"../../../../config.json");

            var config = LoadConfig(configFilePath);

            if(Process.GetProcessesByName(appName).Length <= 0)
                Console.WriteLine($"spyserv track: Application '{appName}' was not found. Try to restart application.");

            else if (!config.AppsToMonitor.Contains(appName))
            {
                config.AppsToMonitor.Add(appName);
                SaveConfig(configFilePath, config);
                Console.WriteLine($"spyserv track: Application '{appName}' added to the config.");
            }
            else
                Console.WriteLine($"spyserv track: Application '{appName}' is already in the config.");
        }

        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json) ?? throw new JsonSerializationException($"Error in desirialization {configFilePath}.");
            }
            else
            {
                return new Config { AppsToMonitor = [] };
            }
        }

        private static void SaveConfig(string configFilePath, Config config)
        {
            var json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configFilePath, json);
        }

        private static void StartWatcherAsDetachedProcess()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var folderPath = Path.Combine(baseDirectory, @"../../.././spyserv-watch/bin/Release/net8.0/spyserv-watch.exe");
            var fullPath = Path.GetFullPath(folderPath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"spyserv start: Watcher binary was not found at {fullPath}");
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
                Console.WriteLine($"spyserv start: Failed to start watcher: {ex.Message}");
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
            var isSpyservApiRunning = IsProcessRunning(StaticClaims.ApiProcessName);
            var isSpyservWatchRunning = IsProcessRunning(StaticClaims.WatcherProcessName);

            if (!isSpyservApiRunning && !isSpyservWatchRunning) Console.WriteLine("spyserv status: SpyServ is not running.");
            else if (isSpyservApiRunning && isSpyservWatchRunning) Console.WriteLine("spyserv status: SpyServ is running.");
        }


        /// <summary>
        /// Removing from config.json application
        /// </summary>
        /// <param name="appName">Application name</param>
        private static void UntrackApplication(string appName)
        {
            var configFilePath = Path.Combine(AppContext.BaseDirectory, @"../../../../config.json");

            var config = LoadConfig(configFilePath);

            if (config.AppsToMonitor.Contains(appName))
            {
                config.AppsToMonitor.Remove(appName);
                SaveConfig(configFilePath, config);
                Console.WriteLine($"spyserv untrack: Application '{appName}' removed from the config.");
            }
            else
            {
                Console.WriteLine($"spyserv untrack: Application '{appName}' is not found in the config.");
            }
        }
    }

    /// <summary>
    /// Class representing config.json
    /// </summary>
    public class Config
    {
        public List<string> AppsToMonitor { get; set; } = [];
    }
}