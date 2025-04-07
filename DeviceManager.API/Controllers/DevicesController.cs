using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private static readonly IDeviceManager DeviceManager = 
        new DeviceManagerService(new InMemoryDeviceRepository());

    [HttpGet]
    public IActionResult GetAllDevices()
    {
        var devices = DeviceManager
            .GetAllDevices()
            .Select(d => new { d.Id, d.Name });

        return Ok(devices);
    }

    [HttpGet("{id}")]
    public IActionResult GetDeviceById(string id)
    {
        var device = DeviceManager.GetDeviceById(id);
        if (device == null)
            return NotFound("Device not found");

        return Ok(device);
    }

    [HttpPost]
    public IActionResult AddDevice([FromBody] Device device)
    {
        try
        {
            DeviceManager.AddDevice(device);
            return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, device);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateDevice(string id, [FromBody] Device updatedDevice)
    {
        var existing = DeviceManager.GetDeviceById(id);
        if (existing == null)
            return NotFound("Device not found.");

        var deviceType = updatedDevice.GetType().Name;

        try
        {
            DeviceManager.EditDevice(updatedDevice.Id, deviceType, updatedDevice.Name);

            if (updatedDevice.IsOn && !existing.IsOn)
                DeviceManager.TurnOnDevice(updatedDevice.Id, deviceType);
            else if (!updatedDevice.IsOn && existing.IsOn)
                DeviceManager.TurnOffDevice(updatedDevice.Id, deviceType);
            
            if (updatedDevice is PersonalComputer pc)
            {
                if (pc.IsOn && string.IsNullOrWhiteSpace(pc.OperatingSystem))
                    throw new EmptySystemException("PC cannot remain turned on without an operating system.");

                DeviceManager.UpdateOperatingSystem(updatedDevice.Id, pc.OperatingSystem);
            }

            if (updatedDevice is Smartwatches sw)
                DeviceManager.UpdateBattery(updatedDevice.Id, sw.BatteryPercentage);

            if (updatedDevice is EmbeddedDevices ed)
            {
                DeviceManager.UpdateIpAddress(updatedDevice.Id, ed.IpName);
                DeviceManager.UpdateNetworkName(updatedDevice.Id, ed.NetworkName);
            }

            return Ok("Device fully updated.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteDevice(string id)
    {
        var device = DeviceManager.GetDeviceById(id);
        if (device == null)
            return NotFound("Device not found.");

        DeviceManager.RemoveDevice(id, device.GetType().ToString().ToLower());
        return NoContent();
    }
}
