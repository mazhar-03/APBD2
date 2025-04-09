using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private static readonly IDeviceManager DeviceManager =
        new DeviceManagerService(new InMemoryDeviceRepository());

    [HttpGet]
    public IResult GetAllDevices()
    {
        var devices = DeviceManager
            .GetAllDevices()
            .Select(d => new { d.Id, d.Name, d.IsOn });

        return Results.Ok(devices);
    }

    [HttpGet("{id}")]
    public IResult GetDeviceById(string id)
    {
        var device = DeviceManager.GetDeviceById(id);
        if (device == null)
            return Results.NotFound("Device not found");

        return Results.Ok(device);
    }

    [HttpPost("pc")]
    public IResult AddPersonalComputer([FromBody] PersonalComputer device)
    {
        DeviceManager.AddDevice(device);
        return Results.Created($"/api/devices/pc/{device.Id}", device);
    }

    [HttpPost("sw")]
    public IResult AddSmartwatch([FromBody] Smartwatches device)
    {
        DeviceManager.AddDevice(device);
        return Results.Created($"/api/devices/sw/{device.Id}", device);
    }

    [HttpPost("ed")]
    public IResult AddEmbeddedDevice([FromBody] EmbeddedDevices device)
    {
        DeviceManager.AddDevice(device);
        return Results.Created($"/api/devices/ed/{device.Id}", device);
    }

    [HttpPut("pc/{id}")]
    public IResult UpdatePersonalComputer(string id, [FromBody] PersonalComputer updatedDevice)
    {
        var existing = DeviceManager.GetDeviceById(id);
        if (existing == null)
            return Results.NotFound("Device not found.");

        try
        {
            EditNameAndTurningOnOffSettings(existing, updatedDevice);
            DeviceManager.UpdateOperatingSystem(updatedDevice.Id, updatedDevice.OperatingSystem);
            return Results.Ok("Device fully updated.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpPut("sw/{id}")]
    public IResult UpdateSmartwatch(string id, [FromBody] Smartwatches updatedDevice)
    {
        var existing = DeviceManager.GetDeviceById(id);
        if (existing == null)
            return Results.NotFound("Device not found.");

        try
        {
            EditNameAndTurningOnOffSettings(existing, updatedDevice);

            DeviceManager.UpdateBattery(updatedDevice.Id, updatedDevice.BatteryPercentage);
            return Results.Ok("Device fully updated.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpPut("ed/{id}")]
    public IResult UpdateEmbeddedDevice(string id, [FromBody] EmbeddedDevices updatedDevice)
    {
        var existing = DeviceManager.GetDeviceById(id);
        if (existing == null)
            return Results.NotFound("Device not found.");

        try
        {
            EditNameAndTurningOnOffSettings(existing, updatedDevice);
            DeviceManager.UpdateIpAddress(updatedDevice.Id, updatedDevice.IpName);
            DeviceManager.UpdateNetworkName(updatedDevice.Id, updatedDevice.NetworkName);
            return Results.Ok("Device fully updated.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IResult DeleteDevice(string id)
    {
        var device = DeviceManager.GetDeviceById(id);
        if (device == null)
            return Results.NotFound("Device not found.");

        DeviceManager.RemoveDevice(id, device.GetType().ToString().ToLower());
        return Results.NoContent();
    }

    private void EditNameAndTurningOnOffSettings(Device? existing, Device? updatedDevice)
    {
        DeviceManager.EditDevice(updatedDevice.Id, updatedDevice.GetType().Name, updatedDevice.Name);
        
        if (updatedDevice.IsOn != existing.IsOn)
        {
            if (updatedDevice.IsOn)
                DeviceManager.TurnOnDevice(updatedDevice.Id, updatedDevice.GetType().Name);
            else
                DeviceManager.TurnOffDevice(updatedDevice.Id, updatedDevice.GetType().Name);
        }
    }
}