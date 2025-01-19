using Newtonsoft.Json;

namespace spyserv_services.Core.Dtos
{
    public class MonitoredApp
    {
        public MonitoredApp() { }

        [JsonConstructor]
        public MonitoredApp(string name, string pathToLogs = "", string description = "", bool isRunning = true, bool autoRestart = false, int checkingIntervalInSec = 60, int restartDelay = 0, bool noNotify = false)
        {
            Name = name;
            PathToLogs = pathToLogs;
            Description = description;
            IsRunning = isRunning;
            AutoRestart = autoRestart;
            CheckingIntervalInSec = checkingIntervalInSec;
            RestartDelay = restartDelay;
            NoNotify = noNotify;
        }

        public required string Name { get; set; }
        public string PathToLogs { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRunning { get; set; } = true;
        public bool AutoRestart { get; set; } = false;
        public required int CheckingIntervalInSec { get; set; } = 60;

        private int _restartDelay = 0;
        public int RestartDelay
        {
            get => _restartDelay;
            set => _restartDelay = (value > 0 || AutoRestart) ? value : 0;
        }

        public bool NoNotify { get; set; } = false;

        public override bool Equals(object? obj) => obj is MonitoredApp app && app.Name == Name;

        public override int GetHashCode() => Name.GetHashCode();
    }
}