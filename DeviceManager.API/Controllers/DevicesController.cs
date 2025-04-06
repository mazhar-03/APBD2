using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/devices")]
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
        var devices = _deviceManager
            .GetAllDevices()
            .Select(d => new { d.Id, d.Name });

        return Ok(devices);
    }

    [HttpGet("{id}")]
    public IActionResult GetDeviceById(string id)
    {
        var device = _deviceManager.GetDeviceById(id);
        if (device == null)
            return NotFound("Device not found");

        return Ok(device);
    }

    [HttpPost]
    public IActionResult AddDevice([FromBody] Device device)
    {
        try
        {
            _deviceManager.AddDevice(device);
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
        var existing = _deviceManager.GetDeviceById(id);
        if (existing == null)
            return NotFound("Device not found.");

        var deviceType = updatedDevice.GetType().Name;

        try
        {
            _deviceManager.EditDevice(updatedDevice.Id, deviceType, updatedDevice.Name);

            if (updatedDevice.IsOn)
                _deviceManager.TurnOnDevice(updatedDevice.Id, deviceType);
            else
                _deviceManager.TurnOffDevice(updatedDevice.Id, deviceType);

            if (updatedDevice is PersonalComputer pc)
            {
                if (pc.IsOn && string.IsNullOrWhiteSpace(pc.OperatingSystem))
                    throw new EmptySystemException("PC cannot remain on without an operating system.");

                _deviceManager.UpdateOperatingSystem(updatedDevice.Id, pc.OperatingSystem);
            }

            if (updatedDevice is Smartwatches sw)
                _deviceManager.UpdateBattery(updatedDevice.Id, sw.BatteryPercentage);

            if (updatedDevice is EmbeddedDevices ed)
            {
                _deviceManager.UpdateIpAddress(updatedDevice.Id, ed.IpName);
                _deviceManager.UpdateNetworkName(updatedDevice.Id, ed.NetworkName);
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
        var device = _deviceManager.GetDeviceById(id);
        if (device == null)
            return NotFound("Device not found.");

        _deviceManager.RemoveDevice(id, device.GetType().ToString().ToLower());
        return NoContent();
    }
}
