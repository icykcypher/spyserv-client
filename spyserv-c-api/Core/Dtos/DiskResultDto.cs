public class DiskResultDto
{
    public string Type { get; } = ResMonType.Disk.ToString();
    public string Device { get; set; } = string.Empty;
    public float ReadMbps { get; set; }
    public float WriteMbps { get; set; }    
}