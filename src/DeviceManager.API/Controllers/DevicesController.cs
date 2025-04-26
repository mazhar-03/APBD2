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
            var summary = _database.GetAllDevices();
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
            
            if (!_database.DeviceExists(id))
                return Results.NotFound($"Device with ID '{id}' not found.");
            
            if (id.StartsWith("p-", StringComparison.OrdinalIgnoreCase))
                device = _database.GetPersonalComputerById(id);
            else if (id.StartsWith("ed-", StringComparison.OrdinalIgnoreCase))
                device = _database.GetEmbeddedDevicesById(id);
            else if (id.StartsWith("sw-", StringComparison.OrdinalIgnoreCase))
                device = _database.GetSmartwatchById(id);

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

                    if (json == null)
                        return Results.BadRequest("Empty or invalid JSON.");

                    var deviceType = json["deviceType"]?.ToString()?.ToLower();
                    var name = json["name"]?.ToString();
                    var isOnNode = json["isOn"];

                    if (string.IsNullOrWhiteSpace(deviceType))
                        return Results.BadRequest("Missing 'deviceType'.");
                    if (string.IsNullOrWhiteSpace(name))
                        return Results.BadRequest("Missing 'name'.");
                    if (isOnNode == null)
                        return Results.BadRequest("Missing 'isOn' field.");

                    bool isOn;
                    try
                    {
                        isOn = isOnNode.GetValue<bool>();
                    }
                    catch
                    {
                        return Results.BadRequest("'isOn' must be true or false.");
                    }

                    switch (deviceType)
                    {
                        case "sw":
                            var batteryNode = json["batteryPercentage"];
                            if (batteryNode == null)
                                return Results.BadRequest("Missing 'batteryPercentage' field for smartwatch.");

                            if (!int.TryParse(batteryNode.ToString(), out var battery))
                                return Results.BadRequest("'batteryPercentage' must be a number.");

                            var sw = new Smartwatches(
                                _database.GenerateNewId("sw"),
                                name,
                                isOn,
                                battery
                            );
                            if (sw.IsOn) sw.TurnOn();
                            if (_database.AddSmartwatch(sw)) return Results.Created();
                            break;

                        case "p":
                            var os = json["operatingSystem"]?.ToString();
                            if (string.IsNullOrWhiteSpace(os))
                                return Results.BadRequest("Missing 'operatingSystem' for personal computer.");

                            var pc = new PersonalComputer(
                                _database.GenerateNewId("p"),
                                name,
                                isOn,
                                os
                            );
                            if (pc.IsOn) pc.TurnOn();
                            if (_database.AddPersonalComputer(pc)) return Results.Created();
                            break;

                        case "ed":
                            var ip = json["ipName"]?.ToString();
                            var network = json["networkName"]?.ToString();
                            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(network))
                                return Results.BadRequest("Missing 'ipName' or 'networkName' for embedded device.");

                            var ed = new EmbeddedDevices(
                                _database.GenerateNewId("ed"),
                                name,
                                isOn,
                                ip,
                                network
                            );
                            if (ed.IsOn) ed.TurnOn();
                            if (_database.AddEmbeddedDevice(ed)) return Results.Created();
                            break;

                        default:
                            return Results.BadRequest("Unsupported deviceType.");
                    }

                    return Results.BadRequest("Device creation failed.");
                }

                case "text/plain":
                {
                    using var reader = new StreamReader(request.Body);
                    var line = await reader.ReadToEndAsync();
                    var parts = line.Split(',');

                    if (parts.Length < 4)
                        return Results.BadRequest("Invalid plain text format. Expected: ID,Name,IsOn,...");

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
                            if (sw.IsOn) sw.TurnOn();
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
                                    "Embedded devices require 5 fields: ID,Name,IsOn,IpName,NetworkName");

                            var ed = new EmbeddedDevices(
                                id,
                                parts[1],
                                bool.Parse(parts[2]),
                                parts[3],
                                parts[4]
                            );
                            if (ed.IsOn) ed.TurnOn();
                            if (_database.AddEmbeddedDevice(ed)) return Results.Created();
                            break;

                        default:
                            return Results.BadRequest("Unsupported device type in plain text import.");
                    }

                    return Results.BadRequest("Device creation from plain text failed.");
                }

                default:
                    return Results.BadRequest("Unsupported Content-Type. Use application/json or text/plain.");
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
                            if (ed.IsOn) ed.TurnOn(); // for triggering ConnectionException 
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
            if (!_database.DeviceExists(id))
                return Results.NotFound($"Device with ID '{id}' not found.");

            var success = false;

            if (id.StartsWith("p-", StringComparison.OrdinalIgnoreCase))
                success = _database.DeletePersonalComputer(id);
            else if (id.StartsWith("sw-", StringComparison.OrdinalIgnoreCase))
                success = _database.DeleteSmartwatch(id);
            else if (id.StartsWith("ed-", StringComparison.OrdinalIgnoreCase))
                success = _database.DeleteEmbeddedDevice(id);

            if (success)
                return Results.Ok("Device deleted successfully.");
            else
                return Results.BadRequest("Failed to delete device." );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error during deletion: {ex.Message}");
        }
    }

}