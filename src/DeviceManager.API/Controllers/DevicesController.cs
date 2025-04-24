using System.Text.Json;
using System.Text.Json.Nodes;
using DeviceManager.Entities;
using DeviceManager.Logic;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

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
        try
        {
            var allDevices = _database
                        .GetAllSmartwatches()
                        .Cast<Device>()
                        .Concat(_database.GetAllPersonalComputers())
                        .Concat(_database.GetAllEmbeddedDevices());
            
                    var summary = allDevices.Select(d => new { d.Id, d.Name, d.IsOn });
            
                    return Results.Ok(summary);
        }
        catch (EmptyBatteryException ex)
        {
            return Results.BadRequest($"Battery error: {ex.Message}");
        }
        catch (EmptySystemException ex)
        {
            return Results.BadRequest($"System error: {ex.Message}");
        }
        catch (ConnectionException ex)
        {
            return Results.BadRequest($"Connection error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest($"Argument error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public IResult GetDeviceById(string id)
    {
        try
        {
            Device? device = null;
                    if (id.StartsWith("p-")) device = _database.GetPersonalComputerById(id.ToLower());
                    else if (id.StartsWith("ed-")) device = _database.GetEmbeddedDevicesById(id.ToLower());
                    else if (id.StartsWith("sw-")) device = _database.GetSmartwatchById(id.ToLower());
            
                    if (device == null) return Results.NotFound("Device not found");
            
                    return Results.Ok(device);
        }
        catch (EmptyBatteryException ex)
        {
            return Results.BadRequest($"Battery error: {ex.Message}");
        }
        catch (EmptySystemException ex)
        {
            return Results.BadRequest($"System error: {ex.Message}");
        }
        catch (ConnectionException ex)
        {
            return Results.BadRequest($"Connection error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest($"Argument error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error: {ex.Message}");
        }
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
                    var line = await reader.ReadToEndAsync();

                    var parts = line.Split(',');
                    if (parts.Length < 4)
                        return Results.BadRequest("Invalid format. Use: ID,Name,IsOn,[additional data depends on the deviceType]");

                    var id = parts[0];
                    var type = id.Split('-')[0].ToLower();

                    switch (type)
                    {
                        case "sw":
                            var sw = new Smartwatches(
                                id,
                                parts[1],
                                bool.Parse(parts[2]),
                                int.Parse(parts[3].Replace("%", ""))
                            );
                            if(sw.IsOn) sw.TurnOn();
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
                                return Results.BadRequest(
                                    "EmbeddedDevices need 5 values: ID,Name,IsOn,IpName,NetworkName");

                            var ed = new EmbeddedDevices(
                                id,
                                parts[1],
                                bool.Parse(parts[2]),
                                parts[3],
                                parts[4]
                            );
                            if(ed.IsOn) ed.TurnOn();
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
        catch (EmptyBatteryException ex)
        {
            return Results.BadRequest($"Battery error: {ex.Message}");
        }
        catch (EmptySystemException ex)
        {
            return Results.BadRequest($"System error: {ex.Message}");
        }
        catch (ConnectionException ex)
        {
            return Results.BadRequest($"Connection error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest($"Argument error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error: {ex.Message}");
        }
    }


    [HttpPut("{id}")]
    public async Task<IResult> UpdateDevice(string id)
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var rawJson = await reader.ReadToEndAsync();

            var json = JsonNode.Parse(rawJson);
            if (json != null)
            {
                json["id"] = id;

                var type = id.Split('-')[0].ToLower();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                switch (type)
                {
                    case "sw":
                        var sw = JsonSerializer.Deserialize<Smartwatches>(json.ToJsonString(), options);
                        if (sw != null)
                        {
                            sw.Id = id;
                            if (sw.IsOn) sw.TurnOn(); // for triggering emptyBatteryEx
                            if (_database.UpdateSmartwatch(id, sw)) return Results.Ok("Smartwatch updated.");
                        }
                        break;
                    case "p":
                        var pc = JsonSerializer.Deserialize<PersonalComputer>(json.ToJsonString(), options);
                        if (pc != null)
                        {
                            pc.Id = id;
                            if (pc.IsOn) pc.TurnOn(); // for triggering emptySystemEx
                            if (_database.UpdatePersonalComputer(id, pc)) return Results.Ok("PC updated.");
                        }

                        break;
                    case "ed":
                        var ed = JsonSerializer.Deserialize<EmbeddedDevices>(json.ToJsonString(), options);
                        if (ed != null)
                        {
                            ed.Id = id;
                            if(ed.IsOn) ed.TurnOn(); // for triggering ConnectionException 
                            if (_database.UpdateEmbeddedDevice(id, ed)) return Results.Ok("Embedded device updated.");
                        }

                        break;

                    default:
                        return Results.BadRequest("Unsupported device type.");
                }
            }

            return Results.BadRequest("Failed to update device.");
        }
        catch (EmptyBatteryException ex)
        {
            return Results.BadRequest($"Battery error: {ex.Message}");
        }
        catch (EmptySystemException ex)
        {
            return Results.BadRequest($"System error: {ex.Message}");
        }
        catch (ConnectionException ex)
        {
            return Results.BadRequest($"Connection error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest($"Argument error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error: {ex.Message}");
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