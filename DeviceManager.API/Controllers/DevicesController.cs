using Microsoft.AspNetCore.Mvc;
using DeviceManager.Entities;
using DeviceManager.Logic;

namespace DeviceManager.API.Controllers
{
    [ApiController]
    [Route("devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceManager _deviceManager;

        public DevicesController(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        [HttpGet]
        public IActionResult GetAllDevices()
        {
            return Ok(_deviceManager.GetAllDevices());
        } 
        
    }
}