using System.Text.Json.Nodes;
using DeviceManager.Entities;
using DeviceManager.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceDBRepository _database;

    public DevicesController(IDeviceDBRepository database)
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
                    if (string.IsNullOrWhiteSpace(deviceType))
                        return Results.BadRequest("Missing 'deviceType'.");

                    var name = json["name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name))
                        return Results.BadRequest("Missing 'name'.");

                    var isOnNode = json["isOn"];
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
                        {
                            var batteryNode = json["batteryPercentage"];
                            if (batteryNode == null)
                                return Results.BadRequest("Missing 'batteryPercentage' field for smartwatch.");

                            int battery;
                            try
                            {
                                battery = batteryNode.GetValue<int>();
                            }
                            catch
                            {
                                return Results.BadRequest("Invalid 'batteryPercentage'. Must be an integer.");
                            }

                            var sw = new Smartwatches(
                                _database.GenerateNewId("sw"),
                                name,
                                false, // Always create OFF first
                                battery
                            );

                            if (isOn)
                                try
                                {
                                    sw.TurnOn();
                                }
                                catch (EmptyBatteryException ex)
                                {
                                    return Results.BadRequest($"Battery error: {ex.Message}");
                                }
                                catch (InvalidOperationException ex)
                                {
                                    return Results.BadRequest($"TurnOn error: {ex.Message}");
                                }

                            if (_database.AddSmartwatch(sw))
                                return Results.Created();

                            break;
                        }

                        case "p":
                        {
                            var os = json["operatingSystem"]?.ToString();
                            if (string.IsNullOrWhiteSpace(os))
                                return Results.BadRequest("Missing 'operatingSystem' for personal computer.");

                            var pc = new PersonalComputer(
                                _database.GenerateNewId("p"),
                                name,
                                false, // Always create OFF first
                                os
                            );

                            if (isOn)
                                try
                                {
                                    pc.TurnOn();
                                }
                                catch (EmptySystemException ex)
                                {
                                    return Results.BadRequest($"System error: {ex.Message}");
                                }
                                catch (InvalidOperationException ex)
                                {
                                    return Results.BadRequest($"TurnOn error: {ex.Message}");
                                }

                            if (_database.AddPersonalComputer(pc))
                                return Results.Created();

                            break;
                        }

                        case "ed":
                        {
                            var ip = json["ipName"]?.ToString();
                            var network = json["networkName"]?.ToString();
                            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(network))
                                return Results.BadRequest("Missing 'ipName' or 'networkName' for embedded device.");

                            var ed = new EmbeddedDevices(
                                _database.GenerateNewId("ed"),
                                name,
                                false, // Always create OFF first
                                ip,
                                network
                            );

                            if (isOn)
                                try
                                {
                                    ed.TurnOn();
                                }
                                catch (ConnectionException ex)
                                {
                                    return Results.BadRequest($"Connection error: {ex.Message}");
                                }
                                catch (InvalidOperationException ex)
                                {
                                    return Results.BadRequest($"TurnOn error: {ex.Message}");
                                }

                            if (_database.AddEmbedded(ed))
                                return Results.Created();

                            break;
                        }

                        default:
                            return Results.BadRequest("Unsupported deviceType.");
                    }

                    return Results.BadRequest("Device creation failed.");
                }
                case "text/plain":
                {
                    using var reader = new StreamReader(Request.Body);
                    var line = await reader.ReadToEndAsync();
                    var parts = line.Split(',');

                    if (parts.Length < 4)
                        return Results.BadRequest("Invalid plain text format. Expected: Type-ID, Name,IsOn,...");

                    var typeId = parts[0];
                    var typePrefix = typeId.Split('-')[0].ToLower();

                    var name = parts[1];
                    var isOnStr = parts[2];

                    if (string.IsNullOrWhiteSpace(name))
                        return Results.BadRequest("Missing 'name' field.");

                    bool isOn;
                    try
                    {
                        isOn = bool.Parse(isOnStr);
                    }
                    catch
                    {
                        return Results.BadRequest("'isOn' must be true or false.");
                    }

                    string newId;
                    try
                    {
                        switch (typePrefix.ToLower())
                        {
                            case "sw":
                                newId = _database.GenerateNewId("sw");
                                var batteryField = parts[3].Trim();
                                if (!batteryField.EndsWith("%"))
                                    return Results.BadRequest("Invalid 'batteryPercentage'. Must end with '%'.");

                                int battery;
                                try
                                {
                                    battery = int.Parse(batteryField.Replace("%", ""));
                                }
                                catch
                                {
                                    return Results.BadRequest("'batteryPercentage' must be a number ending with '%'.");
                                }

                                var sw = new Smartwatches(newId, name, false, battery);
                                if (isOn)
                                    try
                                    {
                                        sw.TurnOn();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Results.BadRequest(ex.Message);
                                    }

                                if (_database.AddSmartwatch(sw))
                                    return Results.Created();
                                break;
                            case "p":
                                newId = _database.GenerateNewId("p");

                                var os = parts[3].Trim();
                                var pc = new PersonalComputer(newId, name, isOn, os);
                                if (isOn)
                                    try
                                    {
                                        pc.TurnOn();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Results.BadRequest(ex.Message);
                                    }

                                if (_database.AddPersonalComputer(pc))
                                    return Results.Created();

                                break;
                            case "ed":
                                newId = _database.GenerateNewId("ed");
                                if (parts.Length < 5)
                                    return Results.BadRequest(
                                        "Embedded devices require 5 fields: ID,Name,IsOn,IpAddress,NetworkName");

                                var ip = parts[3].Trim();
                                var network = parts[4].Trim();

                                var ed = new EmbeddedDevices(newId, name, false, ip, network);

                                if (isOn)
                                    try
                                    {
                                        ed.TurnOn();
                                    }
                                    catch (Exception ex)
                                    {
                                        return Results.BadRequest(ex.Message);
                                    }

                                if (_database.AddEmbedded(ed))
                                    return Results.Created();

                                break;
                            default:
                                return Results.BadRequest("Not supported deviceType.");
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

                    return Results.BadRequest("Device creation from plain text failed.");
                }
                default:
                    return Results.BadRequest("Unsupported Content-Type. Use application/json or text/plain.");
            }
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
            if (!_database.DeviceExists(id))
                return Results.NotFound($"Device with ID '{id}' not found.");

            using var reader = new StreamReader(Request.Body);
            var rawJson = await reader.ReadToEndAsync();

            var json = JsonNode.Parse(rawJson);
            if (json == null)
                return Results.BadRequest("Invalid or empty JSON.");

            var type = id.Split('-')[0].ToLower();

            var name = json["name"]?.ToString();
            var isOnNode = json["isOn"];

            if (string.IsNullOrWhiteSpace(name))
                return Results.BadRequest("Missing 'name' field.");
            if (isOnNode == null)
                return Results.BadRequest("Missing 'isOn' field.");

            bool newIsOn;
            try
            {
                newIsOn = isOnNode.GetValue<bool>();
            }
            catch
            {
                return Results.BadRequest("'isOn' must be true or false.");
            }

            switch (type)
            {
                case "sw":
                {
                    var batteryNode = json["batteryPercentage"];
                    if (batteryNode == null)
                        return Results.BadRequest("Missing 'batteryPercentage' field.");

                    int battery;
                    try
                    {
                        battery = batteryNode.GetValue<int>();
                    }
                    catch
                    {
                        return Results.BadRequest("'batteryPercentage' must be an integer.");
                    }

                    var oldSw = _database.GetSmartwatchById(id);
                    if (oldSw == null)
                        return Results.NotFound($"Smartwatch with ID '{id}' not found.");

                    var sw = new Smartwatches(id, name, oldSw.IsOn, battery);

                    if (!oldSw.IsOn && newIsOn)
                        sw.TurnOn();
                    else
                        sw.IsOn = newIsOn;

                    if (_database.UpdateSmartwatch(id, sw))
                        return Results.Ok("Smartwatch updated.");
                    break;
                }

                case "p":
                {
                    var os = json["operatingSystem"]?.ToString();

                    var oldPc = _database.GetPersonalComputerById(id);
                    if (oldPc == null)
                        return Results.NotFound($"Personal Computer with ID '{id}' not found.");

                    var pc = new PersonalComputer(id, name, oldPc.IsOn, os);

                    if (!oldPc.IsOn && newIsOn)
                        pc.TurnOn();
                    else
                        pc.IsOn = newIsOn;

                    if (_database.UpdatePersonalComputer(id, pc))
                        return Results.Ok("Personal Computer updated.");
                    break;
                }

                case "ed":
                {
                    var ip = json["ipName"]?.ToString();
                    var network = json["networkName"]?.ToString();

                    if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(network))
                        return Results.BadRequest("Missing 'ipName' or 'networkName' fields.");

                    var oldEd = _database.GetEmbeddedDevicesById(id);
                    if (oldEd == null)
                        return Results.NotFound($"Embedded Device with ID '{id}' not found.");

                    var ed = new EmbeddedDevices(id, name, oldEd.IsOn, ip, network);

                    if (!oldEd.IsOn && newIsOn)
                        ed.TurnOn();
                    else
                        ed.IsOn = newIsOn;

                    if (_database.UpdateEmbeddedDevice(id, ed))
                        return Results.Ok("Embedded Device updated.");
                    break;
                }

                default:
                    return Results.BadRequest("Unsupported device type.");
            }

            return Results.BadRequest("Device update failed.");
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
            return Results.BadRequest("Failed to delete device.");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error during deletion: {ex.Message}");
        }
    }
}