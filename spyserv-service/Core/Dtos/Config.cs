namespace spyserv_services.Core.Dtos
{
    /// <summary>
    /// Classes representing config.json
    /// </summary>
    public class Config
    {
        public DebugConfig? Debug { get; set; }
        public ReleaseConfig? Release { get; set; }
        public List<MonitoredApp> MonitoredApps { get; set; } = [];
        public User? User;        
    }

    public class DebugConfig
    {
        public Pathes? Pathes { get; set; }
    }

    public class ReleaseConfig
    {
        public Pathes? Pathes { get; set; }
    }

    public class Pathes
    {
        public string SpyservApi { get; set; } = string.Empty;
        public string SpyservWatcher { get; set; } = string.Empty;
    }
}