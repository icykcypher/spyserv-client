namespace spyserv_services.Core.Dtos
{
    public class MonitoredApp
    {
        public string Name { get; set; } = string.Empty;
        public string PathToLogs { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public bool AutoRestart { get; set; }
    }
}