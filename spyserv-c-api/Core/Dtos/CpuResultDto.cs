public class CpuResultDto
{
    public string Type { get; } = ResMonType.Cpu.ToString();
    public float UsagePercent { get; set; }
}