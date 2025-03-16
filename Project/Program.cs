﻿using System.Text.RegularExpressions;

//tests
/*
// Smartwatches smartwatches = new Smartwatches(1, "apple se", true, 45);

// PersonalComputer pc1 = new PersonalComputer(2, "Test PC", false, "");
// Console.WriteLine(pc1);
// pc1.TurnOn();
// Console.WriteLine(pc1);

// PersonalComputer pc2 = new PersonalComputer(3, "Old PC", false, "    ");
// Console.WriteLine(pc2);
// pc2.TurnOn();
// Console.WriteLine(pc2);

// PersonalComputer pc4 = new PersonalComputer(4, "Office PC", true, "");
// Console.WriteLine(pc4);

// EmbeddedDevices ed1 = new EmbeddedDevices(1, "Sensor", false, "192.168.1.1", "MD Ltd. Network");
// Console.WriteLine(ed1);
// ed1.TurnOn();
// Console.WriteLine(ed1);

// EmbeddedDevices ed2 = new EmbeddedDevices(2, "Camera", false, "999.999.999.999", "MD Ltd. Wifi");

// EmbeddedDevices ed3 = new EmbeddedDevices(3, "Router", false, "192.168.1.100", "Home Network");
// ed3.TurnOn();
*/

//try1
var path = "input.txt";
var manager = new DeviceManager(path);


// addDevice() test
manager.AddDevice("MacBook Pro", 3,false, "macOS"); 
manager.AddDevice( "Raspberry Pi", 2,false, "192.168.1.10", "MD Ltd. Wifi"); 
manager.AddDevice("Garmin Watch", 2, false, "80%"); 
manager.showAllDevices();

public abstract class ElectronicDevice
{
    private int _id;
    private string _name;

    public ElectronicDevice(int id, string name, bool isOn)
    {
        _id = id;
        _name = name;
        IsOn = isOn;
    }

    public int Id
    {
        get => _id;
        set
        {
            if (value < 0)
                throw new ArgumentException("Id cannot be negative");
            _id = value;
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value == null)
                throw new ArgumentException("Name cannot be null");
            _name = value;
        }
    }

    public bool IsOn { get; private set; }


    protected virtual bool CanTurnOn()
    {
        return true; // Default implementation allows turning on
    }

    public virtual void TurnOn()
    {
        if (!CanTurnOn())
            throw new InvalidOperationException("Cannot turn on this device");
        IsOn = true;
    }

    public virtual void TurnOff()
    {
        IsOn = false;
    }

    public override string ToString()
    {
        return $"Device [ID: {Id}, Name: {Name}, Status: {(IsOn ? "On" : "Off")}]";
    }
}


public class Smartwatches : ElectronicDevice, IPowerNotifier
{
    private int _batteryPercentage;

    public Smartwatches(int id, string name, bool isOn, int batteryPercentage) : base(id, name, isOn)
    {
        BatteryPercentage = batteryPercentage;
    }

    public int BatteryPercentage
    {
        get => _batteryPercentage;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Battery percentage must be between 0 and 100");
            _batteryPercentage = value;

            if (_batteryPercentage < 20)
                Notify("Battery is low!!!!");
        }
    }

    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    protected override bool CanTurnOn()
    {
        return _batteryPercentage > 0;
    }

    public override void TurnOn()
    {
        if (!CanTurnOn())
            throw new EmptyBatteryException("Smartwatches cannot turn on with no battery");

        //Since the reqs are say "for setting" we do not use BatteryPercentage
        _batteryPercentage -= 10;
        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | Battery Percentage: {BatteryPercentage}%";
    }
}

public interface IPowerNotifier
{
    void Notify(string notification);
}

public class EmptyBatteryException : Exception
{
    public EmptyBatteryException(string? message) : base(message)
    {
    }
}

public class PersonalComputer : ElectronicDevice
{
    public PersonalComputer(int id, string name, bool isOn, string operationSystem) : base(id, name, isOn)
    {
        OperationSystem = operationSystem;

        if (isOn && string.IsNullOrWhiteSpace(operationSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    public string OperationSystem { get; set; }

    //PC can not turn on without an OS
    protected override bool CanTurnOn()
    {
        return !string.IsNullOrWhiteSpace(OperationSystem);
    }

    //if user try to launch without an OS send our custom exception    
    public override void TurnOn()
    {
        if (!CanTurnOn())
            throw new EmptySystemException("PC can not be launched without an any proper operation system");
        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | Operation System: {OperationSystem}";
    }
}

public class EmptySystemException : Exception
{
    public EmptySystemException(string? message) : base(message)
    {
    }
}

public class EmbeddedDevices : ElectronicDevice
{
    private string _ipAddress;
    private string _networkName;

    public EmbeddedDevices(int id, string name, bool isOn, string ipAddress, string connectionName) : base(id, name,
        isOn)
    {
        IpName = ipAddress;
        NetworkName = connectionName;
    }

    public string IpName
    {
        get => _ipAddress;
        set
        {
            // got the regex values from gpt 
            var regex = new Regex(@"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                  + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                  + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                  + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$");
            if (!regex.IsMatch(value))
                throw new ArgumentException("Invalid IP address specified");
            _ipAddress = value;
        }
    }

    public string NetworkName
    {
        get => _networkName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Network name cannot be empty.");
            _networkName = value;
        }
    }

    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException("Cannot connect to a device. Must have MD Ltd.");
        Console.WriteLine($"Connecting to device: {NetworkName}");
    }

    protected override bool CanTurnOn()
    {
        return NetworkName.Contains("MD Ltd.");
    }

    public override void TurnOn()
    {
        Connect();
        if (!CanTurnOn())
            throw new ConnectionException("Device cannot be turned on due to network restrictions.");

        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | IP: {_ipAddress} - Network: {NetworkName}";
    }
}

public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message)
    {
    }
}

public class DeviceManager
{
    private const int MaxNumOfDevices = 15;
    private readonly List<ElectronicDevice> _devices = new();

    public DeviceManager(string FilePath)
    {
        if (!File.Exists(FilePath))
            throw new FileNotFoundException("File does not exist");


        var lines = File.ReadAllLines(FilePath);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                Console.WriteLine($"Skipping invalid line: {line}");
                continue;
            }

            var typeId = parts[0];
            var typeAndIdArr = typeId.Split('-');

            if (typeAndIdArr.Length != 2)
            {
                Console.WriteLine($"Skipping invalid type-id format: {line}");
                continue;
            }

            var type = typeAndIdArr[0];
            var id = int.Parse(typeAndIdArr[1]);
            var name = parts[1];
            var isOn = false;
            if (type == "SW" || type == "P")
            {
                if (!(parts[2] == "false" || parts[2] == "true"))
                {
                    Console.WriteLine($"Skipping invalid isOn value: {line}");
                    continue;
                }

                isOn = bool.Parse(parts[2]);
            }

            if (_devices.Count >= MaxNumOfDevices)
                Console.WriteLine($"Max number of devices: {MaxNumOfDevices}\nCannot add more devices.");

            switch (type)
            {
                case "SW":
                    if (parts.Length < 4 || !parts[3].Contains("%"))
                    {
                        Console.WriteLine($"Skipping invalid smartwatch data: {line}");
                        continue;
                    }

                    var batteryPercentage = int.Parse(parts[3].TrimEnd('%'));
                    _devices.Add(new Smartwatches(id, name, isOn, batteryPercentage));
                    break;
                case "P":
                    if (parts.Length <= 4)
                    {
                        Console.WriteLine($"Skipping invalid PC data (missing OS): {line}");
                        continue;
                    }

                    _devices.Add(new PersonalComputer(id, name, isOn, parts[3]));
                    break;
                case "ED":
                    if (parts.Length <= 5)
                    {
                        Console.WriteLine($"Skipping invalid embedded device data: {line}");
                        continue;
                    }

                    _devices.Add(new EmbeddedDevices(id, name, isOn, parts[3], parts[4]));
                    break;
                default:
                    Console.WriteLine($"Skipping unknown device type: {line}");
                    break;
            }
        }
    }

    public void showAllDevices()
    {
        Console.WriteLine("\nDevices:");
        foreach (var device in _devices) Console.WriteLine(device);
    }

    public void AddDevice(string name, int id, bool isOn, string data, string networkName = null)
    {
        if (_devices.Count >= MaxNumOfDevices)
        {
            Console.WriteLine("Cannot add new device. Storage is full.");
            return;
        }

        string deviceType = null;
        if(data.EndsWith('%')) deviceType = "SW";
        else if(IsValidIp(data) && !string.IsNullOrWhiteSpace(networkName)) deviceType = "ED";
        else if(!string.IsNullOrWhiteSpace(data)) deviceType = "P";
        else
        { 
            Console.WriteLine("Invalid data format. Could not determine device type.");
            return;
        }
        
        if (_devices.Any(d => GetDeviceType(d) == deviceType && d.Id == id))
        {
            Console.WriteLine($"A {deviceType} with ID {id} already exists.");
            return;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Device name cannot be empty.");
            return;
        }

        ElectronicDevice newDevice = null;
        switch (deviceType)
        {
            case "SW":
                if (!int.TryParse(data.TrimEnd('%'), out int battery) || battery < 0 || battery > 100)
                {
                    Console.WriteLine("Invalid battery percentage.");
                    return;
                }
                newDevice = new Smartwatches(id, name, isOn, battery);
                break;

            case "P": 
                newDevice = new PersonalComputer(id, name, isOn, data);
                break;

            case "ED":  
                newDevice = new EmbeddedDevices(id, name, false, data, networkName);
                break;
        }
        if(newDevice != null)
        {
            _devices.Add(newDevice);
            Console.WriteLine($"{newDevice.GetType().Name} added successfully!");
        }
    }

    private bool IsValidIp(string value)
    {
        var regex = new Regex(@"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                              + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                              + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                              + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$");

        return regex.IsMatch(value);
    }
    private string GetDeviceType(ElectronicDevice device)
    {
        if (device is Smartwatches) return "SW";
        if (device is PersonalComputer) return "P";
        if (device is EmbeddedDevices) return "ED";
        return "Unknown";
    }
}