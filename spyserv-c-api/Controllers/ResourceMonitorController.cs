using spyserv_c_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace spyserv_c_api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ResourceMonitorController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() 
        {
            return Ok();
        }
        [HttpGet("cpu")]
        public IActionResult GetCpuUsage()
        {
            var cpuUsage = ResourceMonitorService.GetCpuUsagePercentage();
            return Ok(new { CpuUsage = cpuUsage });
        }

        // Получить информацию о памяти
        [HttpGet("memory")]
        public IActionResult GetMemoryUsage()
        {
            var memoryUsage = ResourceMonitorService.GetMemoryUsage();
            return Ok(new { UsedMemory = memoryUsage.Item1, TotalMemory = memoryUsage.Item2 });
        }

        // Получить информацию о дисках
        [HttpGet("disk")]
        public IActionResult GetDiskUsage()
        {
            var diskUsage = ResourceMonitorService.GetDiskUsage();
            return Ok(new
            {
                UsedSpace = diskUsage.Item1,
                FreeSpace = diskUsage.Item2,
                TotalSpace = diskUsage.Item3
            });
        }
    }
}