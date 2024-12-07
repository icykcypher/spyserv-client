using spyserv_client;
using Newtonsoft.Json;
using System.Diagnostics;

namespace spyserv
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args is null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                {
                    Console.WriteLine("spyserv: Unknown command");
                    return;
                }
                switch (args[0].ToLower())
                {
                    case "start":
                        StartApiAsDetachedProcess();
                        StartWatcherAsDetachedProcess();
                        break;

                    case "status":
                        ShowStatus();
                        break;

                    case "help":
                        ShowCommands();
                        break;

                    case "stop":
                        StopApi();
                        StopSpy();
                        break;

                    case "track":
                        TrackApplication(args);
                        break;

                    case "untrack":
                        UntrackApplication(args[1]);
                        break;

                    case "config":
                    // TODO 
                    // Implement
                        throw new NotImplementedException();
                        break;
                    default:
                        Console.WriteLine($"spyserv: Unknown command: {args[0]}");
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"spyserv: {e.Message}");
            }
        }
        private static void StartApiAsDetachedProcess()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var folderPath = Path.Combine(baseDirectory, @"../../../../spyserv-c-api/bin/Release/net8.0/spyserv-c-api");
            var fullPath = Path.GetFullPath(folderPath);

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
                Console.WriteLine($"spyserv: Failed to start API: {ex.Message}");
            }
        }

        private static void StopApi()
        {
            try
            {
                var processes = Process.GetProcessesByName(StaticClaims.ApiProcessName);

                if (processes.Length > 0) foreach (var process in processes) process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv: Error while trying to kill process: {ex.Message}");
            }
        }

        private static void StopSpy()
        {
            try
            {
                var processes = Process.GetProcessesByName(StaticClaims.SpyProcessName);

                if (processes.Length > 0) foreach (var process in processes) process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"spyserv: Error while trying to kill process: {ex.Message}");
            }
        }

        private static void ShowCommands()
        {
            Console.WriteLine("spyserv: Available commands:");
            Console.WriteLine("  start              - Start the web api and system watcher applications");
            Console.WriteLine("  status             - Show application status");
            Console.WriteLine("  help               - Show available commands");
            Console.WriteLine("  track [app-name]   - Adding application to monitored apps");
            Console.WriteLine("  untrack [app-name] - Removing application from monitored apps");
            Console.WriteLine("  stop               - Stop application");
        }

        private static void TrackApplication(string[] args)
        {
            if (args.Length > 1) AddAppToConfig(args[1]);
            else Console.WriteLine("spyserv: Invalid arguments for configure.");
        }

        private static void AddAppToConfig(string appName)
        {
            string configFilePath = Path.Combine(AppContext.BaseDirectory, @"../../../../config.json");

            var config = LoadConfig(configFilePath);

            if(Process.GetProcessesByName(appName).Length <= 0)
                Console.WriteLine($"spyserv: Application '{appName}' was not found. Try to restart application.");

            else if (!config.AppsToMonitor.Contains(appName))
            {
                config.AppsToMonitor.Add(appName);
                SaveConfig(configFilePath, config);
                Console.WriteLine($"spyserv: Application '{appName}' added to the config.");
            }
            else
                Console.WriteLine($"spyserv: Application '{appName}' is already in the config.");
        }

        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                return new Config { AppsToMonitor = new List<string>() };
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
                Console.WriteLine($"spyserv: Failed to start watcher: {ex.Message}");
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

        private static void ShowStatus()
        {
            bool isSpyservApiRunning = IsProcessRunning(StaticClaims.ApiProcessName);
            bool isSpyservWatchRunning = IsProcessRunning(StaticClaims.SpyProcessName);

            if (!isSpyservApiRunning && !isSpyservWatchRunning) Console.WriteLine("spyserv: SpyServ is not running.");
            else if (isSpyservApiRunning && isSpyservWatchRunning) Console.WriteLine("spyserv: SpyServ is running.");
        }

        private static void UntrackApplication(string appName)
        {
            string configFilePath = Path.Combine(AppContext.BaseDirectory, @"../../../../config.json");

            var config = LoadConfig(configFilePath);

            if (config.AppsToMonitor.Contains(appName))
            {
                config.AppsToMonitor.Remove(appName);
                SaveConfig(configFilePath, config);
                Console.WriteLine($"spyserv: Application '{appName}' removed from the config.");
            }
            else
            {
                Console.WriteLine($"spyserv: Application '{appName}' is not found in the config.");
            }
        }

    }
    public class Config
    {
        public List<string> AppsToMonitor { get; set; } = [];
    }
}