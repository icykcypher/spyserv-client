public class MemoryResultDto
{
    public string Type { get; } = ResMonType.Memory.ToString();
    public float UsedPercent { get; set; }
    public float TotalMemoryMb { get; set; }
}