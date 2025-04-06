using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

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
        var devices = _deviceManager.
            GetAllDevices().
            Select(d => new {d.Id, d.Name});
        
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

        var deviceType = existing.GetType().Name;

        try
        {
            if (updatedDevice.Name != existing.Name)
                _deviceManager.EditDevice(id, deviceType, updatedDevice.Name);

            if (updatedDevice.IsOn && !existing.IsOn)
                _deviceManager.TurnOnDevice(id, deviceType);
            else if (!updatedDevice.IsOn && existing.IsOn)
                _deviceManager.TurnOffDevice(id, deviceType);

            if (existing is PersonalComputer && updatedDevice is PersonalComputer pc)
                _deviceManager.UpdateOperatingSystem(id, pc.OperatingSystem);

            if (existing is Smartwatches && updatedDevice is Smartwatches sw)
                _deviceManager.UpdateBattery(id, sw.BatteryPercentage);

            if (existing is EmbeddedDevices && updatedDevice is EmbeddedDevices ed)
            {
                _deviceManager.UpdateIpAddress(id, ed.IpName);
                _deviceManager.UpdateNetworkName(id, ed.NetworkName);
            }

            return Ok("Device updated.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteDevice(string id)
    {
        var device = _deviceManager.GetDeviceById(id);
        if(device == null)
            return NotFound("Device not found.");
        
        _deviceManager.RemoveDevice(id, device.GetType().Name);
        return NoContent();
    }
}