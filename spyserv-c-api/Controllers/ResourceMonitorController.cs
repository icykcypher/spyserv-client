using spyserv_c_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace spyserv_c_api.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ResourceMonitorController(Serilog.ILogger logger) : ControllerBase
    {
        private readonly Serilog.ILogger _logger = logger;

        [HttpGet("cpu")]
        public IActionResult GetCpuUsage()
        {
            try
            {
                var cpuUsage = ResourceMonitorService.GetCpuUsagePercentage();
                return Ok(cpuUsage);
            }
            catch(Exception e)
            {
                _logger.Error($"{e.Message}");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("memory")]
        public IActionResult GetMemoryUsage()
        {
            try
            {
                var memoryUsage = ResourceMonitorService.GetMemoryUsage();
                return Ok(memoryUsage);
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("disk")]
        public IActionResult GetDiskUsage()
        {
            try
            {
                var diskUsage = ResourceMonitorService.GetDiskUsage();
                return Ok(diskUsage);
            }
            catch (Exception e)
            {
                _logger.Error($"{e.Message}");
                return StatusCode(500, e.Message);
            }
        }
    }
}