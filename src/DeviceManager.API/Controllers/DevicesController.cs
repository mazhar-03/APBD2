using System.Text.Json.Nodes;
using DeviceManager.Entities;
using DeviceManager.Entities.DTO;
using DeviceManager.Logic;
using DeviceManager.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;

    public DevicesController(IDeviceService service)
    {
        _service = service;
    }

    [HttpGet]
    public IResult GetAllDevices()
    {
        try
        {
            var devices = _service.GetAllDevices().Select(d => new { d.Id, d.Name, d.IsEnabled });

            return Results.Ok(devices);
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
            DeviceDto? device = null;

            if (!_service.DeviceExists(id))
                return Results.NotFound($"Device with ID '{id}' not found.");

            device = _service.GetDeviceById(id);

            if (device == null)
                return Results.NotFound($"Device with ID '{id}' could not be retrieved.");

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
                                _service.GenerateNewId("sw"),
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

                            if (_service.AddSmartwatch(sw))
                                return Results.Created();

                            break;
                        }

                        case "p":
                        {
                            var os = json["operatingSystem"]?.ToString();
                            if (string.IsNullOrWhiteSpace(os))
                                return Results.BadRequest("Missing 'operatingSystem' for personal computer.");

                            var pc = new PersonalComputer(
                                _service.GenerateNewId("p"),
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

                            if (_service.AddPersonalComputer(pc))
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
                                _service.GenerateNewId("ed"),
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

                            if (_service.AddEmbedded(ed))
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
                                newId = _service.GenerateNewId("sw");
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

                                if (_service.AddSmartwatch(sw))
                                    return Results.Created();
                                break;
                            case "p":
                                newId = _service.GenerateNewId("p");

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

                                if (_service.AddPersonalComputer(pc))
                                    return Results.Created();

                                break;
                            case "ed":
                                newId = _service.GenerateNewId("ed");
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

                                if (_service.AddEmbedded(ed))
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
            var deviceDto = _service.GetDeviceById(id);
            if (deviceDto == null)
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

                    if (deviceDto is SmartwatchDto swDto)
                    {
                        var swEntity = new Smartwatches(
                            swDto.Id,
                            swDto.Name,
                            swDto.IsEnabled,
                            battery
                        );

                        if (!swEntity.IsOn && newIsOn)
                            try
                            {
                                swEntity.TurnOn();
                            }
                            catch (EmptyBatteryException ex)
                            {
                                return Results.BadRequest($"Battery error: {ex.Message}");
                            }

                        swDto.BatteryLevel = battery;
                        swDto.IsEnabled = newIsOn;

                        var result = _service.UpdateDevice(swDto);
                        if (result)
                            return Results.Ok("Smartwatch updated.");
                        return Results.BadRequest("Smartwatch update failed.");
                    }

                    break;
                }

                case "p":
                {
                    var os = json["operatingSystem"]?.ToString();
                    if (string.IsNullOrWhiteSpace(os))
                        return Results.BadRequest("Missing 'operatingSystem' field.");

                    if (deviceDto is PersonalComputerDto pcDto)
                    {
                        var pcEntity = new PersonalComputer(
                            pcDto.Id,
                            pcDto.Name,
                            pcDto.IsEnabled,
                            os
                        );

                        if (!pcEntity.IsOn && newIsOn)
                            pcEntity.TurnOn();
                        else
                            pcEntity.IsOn = newIsOn;

                        pcDto.OperatingSystem = os;

                        var result = _service.UpdateDevice(pcDto);
                        if (result)
                            return Results.Ok("Personal Computer updated.");
                        return Results.BadRequest("Personal Computer update failed.");
                    }

                    break;
                }

                case "ed":
                {
                    var ip = json["ipName"]?.ToString();
                    var network = json["networkName"]?.ToString();

                    if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(network))
                        return Results.BadRequest("Missing 'ipName' or 'networkName' fields.");

                    if (deviceDto is EmbeddedDto edDto)
                    {
                        var edEntity = new EmbeddedDevices(
                            edDto.Id,
                            edDto.Name,
                            edDto.IsEnabled,
                            ip,
                            network
                        );

                        if (!edEntity.IsOn && newIsOn)
                            edEntity.TurnOn();
                        else
                            edEntity.IsOn = newIsOn;

                        edDto.IpAddress = ip;
                        edDto.NetworkName = network;

                        var result = _service.UpdateDevice(edDto);
                        if (result)
                            return Results.Ok("Embedded Device updated.");
                        return Results.BadRequest("Embedded Device update failed.");
                    }

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
            if (!_service.DeviceExists(id))
                return Results.NotFound($"Device with ID '{id}' not found.");

            var success = _service.DeleteDevice(id);

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