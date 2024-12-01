using Serilog;
using Newtonsoft.Json;
using System.Diagnostics;

namespace spyserv_watch
{
    public class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, @"../../../../../logs/spyserv-watch.log"), rollingInterval: RollingInterval.Day)  // Логирование в файл
                .CreateLogger();

            Log.Information("App started. Current Directory: {Directory}", AppContext.BaseDirectory);

            StartMonitoring();
        }

        public static void StartMonitoring()
        {
            while (true)
            {
                var appNames = GetAppsToMonitor();
                foreach (var appName in appNames) CheckApplicationStatus(appName);

                Thread.Sleep(60000);
            }
        }

        private static List<string> GetAppsToMonitor()
        {
            var config = LoadConfig(@"../../../../config.json");
            return config.AppsToMonitor;
        }

        private static void CheckApplicationStatus(string appName)
        {
            var processes = Process.GetProcessesByName(appName);
            string status = processes.Length > 0 ? "running" : "not running";

            if (status == "not running")
            {
                Log.Error($"Application '{appName}' has stopped working!");
            }
        }

        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            else
                return new Config { AppsToMonitor = new List<string>() };
        }
    }

    public class Config
    {
        public List<string> AppsToMonitor { get; set; } = new List<string>();
    }
}