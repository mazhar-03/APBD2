using System.Text.Json;
using System.Text.Json.Nodes;
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

    
[HttpPost]
public async Task<IResult> AddDevice()
{
    var request = Request;
    var contentType = request.ContentType?.ToLower();

    try
    {
        switch (contentType)
        {
            case "application/json":
            {
                using var reader = new StreamReader(request.Body);
                var rawJson = await reader.ReadToEndAsync();

                var json = JsonNode.Parse(rawJson);
                if (json == null) return Results.BadRequest("NULL JSON.");

                var id = json["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                    return Results.BadRequest("Missing 'id' field.");

                var type = id.Split('-')[0].ToLower();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                switch (type)
                {
                    case "sw":
                        var sw = JsonSerializer.Deserialize<Smartwatches>(json.ToJsonString(), options);
                        if (sw != null && _database.AddSmartwatch(sw)) return Results.Created();
                        break;

                    case "p":
                        var pc = JsonSerializer.Deserialize<PersonalComputer>(json.ToJsonString(), options);
                        if (pc != null)
                        {
                            if (pc.IsOn) pc.TurnOn();
                            if (_database.AddPersonalComputer(pc)) return Results.Created();
                        }
                        break;

                    case "ed":
                        var ed = JsonSerializer.Deserialize<EmbeddedDevices>(json.ToJsonString(), options);
                        if (ed != null && _database.AddEmbeddedDevice(ed)) return Results.Created();
                        break;

                    default:
                        return Results.BadRequest("Unsupported device type.");
                }

                return Results.BadRequest("Failed to insert device.");
            }

            case "text/plain":
            {
                using var reader = new StreamReader(request.Body);
                string line = await reader.ReadToEndAsync();

                string[] parts = line.Split(',');
                if (parts.Length < 4)
                    return Results.BadRequest("Invalid format. Use: ID,Name,IsOn,[...additional data]");

                string id = parts[0];
                string type = id.Split('-')[0].ToLower();

                switch (type)
                {
                    case "sw":
                        var sw = new Smartwatches(
                            id,
                            parts[1],
                            bool.Parse(parts[2]),
                            int.Parse(parts[3].Replace("%", ""))
                        );
                        if (_database.AddSmartwatch(sw)) return Results.Created();
                        break;

                    case "p":
                        var pc = new PersonalComputer(
                            id,
                            parts[1],
                            bool.Parse(parts[2]),
                            parts[3]
                        );
                        if (pc.IsOn) pc.TurnOn();
                        if (_database.AddPersonalComputer(pc)) return Results.Created();
                        break;

                    case "ed":
                        if (parts.Length < 5)
                            return Results.BadRequest("EmbeddedDevices need 5 values: ID,Name,IsOn,IpName,NetworkName");

                        var ed = new EmbeddedDevices(
                            id,
                            parts[1],
                            bool.Parse(parts[2]),
                            parts[3],
                            parts[4]
                        );
                        if (_database.AddEmbeddedDevice(ed)) return Results.Created();
                        break;

                    default:
                        return Results.BadRequest("Unsupported device type.");
                }

                return Results.BadRequest("Failed to insert device.");
            }

            default:
                return Results.Conflict("Unsupported device type.");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
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