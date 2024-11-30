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
            return Ok(cpuUsage);
        }

        [HttpGet("memory")]
        public IActionResult GetMemoryUsage()
        {
            var memoryUsage = ResourceMonitorService.GetMemoryUsage();
            return Ok(memoryUsage);
        }

        [HttpGet("disk")]
        public IActionResult GetDiskUsage()
        {
            var diskUsage = ResourceMonitorService.GetDiskUsage();
            return Ok(diskUsage);
        }
    }
}