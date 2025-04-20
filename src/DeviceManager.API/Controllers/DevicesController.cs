using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDatabaseService _database;

    public DevicesController(IDatabaseService database)
    {
        _database = database;
    }

    [HttpGet]
    public IResult GetAllDevices()
    {
        var allDevices = _database
            .GetAllSmartwatches()
            .Cast<Device>()
            .Concat(_database.GetAllPersonalComputers())
            .Concat(_database.GetAllEmbeddedDevices());

        var summary = allDevices.Select(d => new { d.Id, d.Name, d.IsOn });

        return Results.Ok(summary);
    }

    [HttpGet("{id}")]
    public IResult GetDeviceById(string id)
    {
        Device device = null;
        if (id.StartsWith("p-")) device = _database.GetPersonalComputerById(id.ToLower());
        else if (id.StartsWith("ed-")) device = _database.GetEmbeddedDevicesById(id.ToLower());
        else if (id.StartsWith("sw-")) device = _database.GetSmartwatchById(id.ToLower());

        if (device == null) return Results.NotFound("Device not found");

        return Results.Ok(device);
    }

    [HttpPost("pc")]
    public IResult AddPersonalComputer([FromBody] PersonalComputer device)
    {
        try
        {
            var success = _database.AddPersonalComputer(device);

            if (success) return Results.Created();
            return Results.BadRequest();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    [HttpPost("sw")]
    public IResult AddSmartwatch([FromBody] Smartwatches device)
    {
        try
        {
            var success = _database.AddSmartwatch(device);

            if (success) return Results.Created();
            return Results.BadRequest();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    [HttpPost("ed")]
    public IResult AddEmbeddedDevice([FromBody] EmbeddedDevices device)
    {
        try
        {
            var success = _database.AddEmbeddedDevice(device);

            if (success) return Results.Created();
            return Results.BadRequest();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    [HttpPut("pc/{id}")]
    public IResult UpdatePersonalComputer(string id, [FromBody] PersonalComputer updatedDevice)
    {
        try
        {
            var success = _database.UpdatePersonalComputer(id, updatedDevice);
            return success ? Results.Ok("Device fully updated.") : Results.NotFound("Device not found.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpPut("sw/{id}")]
    public IResult UpdateSmartwatch(string id, [FromBody] Smartwatches updatedDevice)
    {
        try
        {
            var success = _database.UpdateSmartwatch(id, updatedDevice);
            return success ? Results.Ok("Device fully updated.") : Results.NotFound("Device not found.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpPut("ed/{id}")]
    public IResult UpdateEmbeddedDevice(string id, [FromBody] EmbeddedDevices updatedDevice)
    {
        try
        {
            var success = _database.UpdateEmbeddedDevice(id, updatedDevice);
            return success ? Results.Ok("Device fully updated.") : Results.NotFound("Device not found.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Update failed: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IResult DeleteDevice(string id)
    {
        try
        {
            var success = false;
            if (id.StartsWith("p-")) success = _database.DeletePersonalComputer(id.ToLower());
            else if (id.StartsWith("sw-")) success = _database.DeleteSmartwatch(id.ToLower());
            else if (id.StartsWith("ed-")) success = _database.DeleteEmbeddedDevice(id.ToLower());
            return success ? Results.Ok("Device deleted") : Results.NotFound("Device not found.");
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Delete failed: {ex.Message}");
        }
    }
}