namespace spyserv_services.Core.Dtos
{
    public class MonitoringData
    {
        public required CpuResultDto CpuResult { get; set; }
        public required MemoryResultDto MemoryResult { get; set; }
        public required DiskResultDto DiskResult { get; set; }
    }
}