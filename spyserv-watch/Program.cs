using Newtonsoft.Json;
using System.Diagnostics;

namespace spyserv_watch
{
    public class Program
    {
        static void Main(string[] args)
        {
            StartMonitoring();
        }
        public static void StartMonitoring()
        {
            var appNames = GetAppsToMonitor();

            while (true)
            {
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
                LogStatus(appName, status);
            }
        }

        private static void LogStatus(string appName, string status)
        {
            var logFilePath = Path.Combine(AppContext.BaseDirectory, @"../../../../monitoring_log.log");
            var logMessage = $"{DateTime.Now}: Application '{appName}' has stopped working!";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            Console.WriteLine(logMessage); 
        }
        private static Config LoadConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            else return new Config { AppsToMonitor = new List<string>() };
        }
    }
    public class Config
    {
        public List<string> AppsToMonitor { get; set; } = [];
    }
}