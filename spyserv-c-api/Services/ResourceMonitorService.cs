using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace spyserv_c_api.Services
{
    public static class ResourceMonitorService
    {
        public static CpuResultDto GetCpuUsagePercentage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return JsonConvert.DeserializeObject<CpuResultDto>(GetCommandOutput("./scripts/resmon", "cpu")) 
                ?? throw new Exception("Can not deserialize resmon.");
            }
            else throw new NotImplementedException();
        }

        public static MemoryResultDto GetMemoryUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var resStr = GetCommandOutput("./scripts/resmon", "cpu");

                return JsonConvert.DeserializeObject<MemoryResultDto>(resStr) ?? throw new Exception("Can not deserialize resmon.");
            }
            else throw new NotImplementedException();
        }

        public static DiskResultDto GetDiskUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var device = GetMainDiskFromDf();
                var resStr = GetCommandOutput("./scripts/resmon", $"disk {device}");
                
                return JsonConvert.DeserializeObject<DiskResultDto>(resStr) ?? throw new Exception("Can not deserialize resmon.");
            }
            else throw new NotImplementedException();
        }

        public static string GetMainDiskFromDf()
        {
            var output = GetCommandOutput("df", "-h");

            var lines = output.Split("\n").Where(line => line.Contains("/"));
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var device = parts[0];
                return device;
            }

            return null;
        }
        private static string GetCommandOutput(string command, string args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processInfo)) return process.StandardOutput.ReadToEnd();
        }
    }
}