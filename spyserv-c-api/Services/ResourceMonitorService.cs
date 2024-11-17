using System.Diagnostics;
using System.Runtime.InteropServices;

namespace spyserv_c_api.Services
{
    public static class ResourceMonitorService
    {
        public static float GetCpuUsagePercentage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var process = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                process.NextValue();
                Thread.Sleep(1000);
                return process.NextValue();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return 0;
            }
            else return -1;
        }

        // Метод для получения использования памяти
        public static (float, float) GetMemoryUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var totalMemory = new PerformanceCounter("Memory", "Commit Limit");
                var usedMemory = new PerformanceCounter("Memory", "Committed Bytes");
                return (usedMemory.NextValue() / totalMemory.NextValue() * 100, totalMemory.NextValue());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return (0, 0);
            }
            else return (-1, -1);
        }

        public static (long, long, long) GetDiskUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
                if (drive != null)
                {
                    return ((drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024 / 1024, drive.AvailableFreeSpace / 1024 / 1024 / 1024
                        , drive.TotalSize / 1024 / 1024 / 1024);
                }
                return (0, 0, 0);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return (0, 0, 0);
            }
            else return (-1, -1, -1);
        }
    }
}