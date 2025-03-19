using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

DeviceManager manager = new DeviceManager("devices.txt");
manager.ShowAllDevices();
manager.AddDevice(new PersonalComputer(2, "Huawei", true, "Windows"));
manager.ShowAllDevices();

public abstract class ElectronicDevice
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }

    public ElectronicDevice(int id, string name, bool isOn)
    {
        Id = id < 0 ? throw new ArgumentException("Id cannot be negative") : id;
        Name = name ?? throw new ArgumentException("Name cannot be null");
        IsOn = isOn;
    }

    public virtual void TurnOn() => IsOn = true;
    public void TurnOff() => IsOn = false;

    public override string ToString() => $"Device [ID: {Id}, Name: {Name}, Status: {(IsOn ? "On" : "Off")}]";
}

public class Smartwatches : ElectronicDevice, IPowerNotifier
{
    private int _batteryPercentage;
    private bool _notifiedLowBattery;

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
            if (_batteryPercentage < 20 && !_notifiedLowBattery)
            {
                Notify("Smartwatch's battery is low!");
                _notifiedLowBattery = true;
            }
        }
    }

    public void Notify(string msg) => Console.WriteLine(msg);

    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
            throw new EmptyBatteryException("Smartwatches cannot turn on with battery less than 11%");
        BatteryPercentage -= 10;
        base.TurnOn();
    }

    public override string ToString() => $"SW-{Id},{Name},{IsOn},{BatteryPercentage}%";
}

public interface IPowerNotifier { void Notify(string notification); }

public class EmptyBatteryException : Exception { public EmptyBatteryException(string? message) : base(message) { } }

public class PersonalComputer : ElectronicDevice
{
    public string OperationSystem { get; set; }

    public PersonalComputer(int id, string name, bool isOn, string operationSystem) : base(id, name, isOn)
    {
        OperationSystem = operationSystem;
        if (isOn && string.IsNullOrWhiteSpace(operationSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperationSystem))
            throw new EmptySystemException("PC cannot be launched without an operating system.");
        base.TurnOn();
    }

    public override string ToString() => $"P-{Id},{Name},{IsOn},{OperationSystem}";
}

public class EmptySystemException : Exception { public EmptySystemException(string? message) : base(message) { } }

public class EmbeddedDevices : ElectronicDevice
{
    private string _ipAddress;
    private string _networkName;

    public EmbeddedDevices(int id, string name, bool isOn, string ipAddress, string connectionName) : base(id, name, isOn)
    {
        IpName = ipAddress;
        NetworkName = connectionName;
    }

    public string IpName
    {
        get => _ipAddress;
        set
        {
            if (!Regex.IsMatch(value, @"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                        @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                        @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                        @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$"))
                throw new ArgumentException("Invalid IP address specified");
            _ipAddress = value;
        }
    }

    public string NetworkName
    {
        get => _networkName;
        set
        {
            if (!value.Contains("MD Ltd."))
                throw new ConnectionException("Invalid network name! Must connect to 'MD Ltd.'.");
            _networkName = value;
        }
    }

    public override string ToString() => $"ED-{Id},{Name},{IsOn},{IpName},{NetworkName}";
}

public class ConnectionException : Exception { public ConnectionException(string message) : base(message) { } }

public class DeviceManager
{
    private const int MaxNumOfDevices = 15;
    private List<ElectronicDevice> _devices = new();
    private string filePath;

    public DeviceManager(string filePath)
    {
        this.filePath = filePath;
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);
        HashSet<int> existingIds = new();

        foreach (string line in lines)
        {
            try
            {
                var device = ParseDevice(line);
                if (device != null && existingIds.Add(device.Id))
                    _devices.Add(device);
            }
            catch { }
        }
    }

    private ElectronicDevice ParseDevice(string line)
    {
        string[] parts = line.Split(',');

        if (parts.Length < 3)
            throw new FormatException("Incomplete device data.");

        var typeAndId = parts[0].Split('-');
        var type = typeAndId[0];
        var id = int.Parse(typeAndId[1]);
        var name = parts[1];
        var isOn = bool.Parse(parts[2]);

        return type switch
        {
            "SW" => new Smartwatches(id, name, isOn, int.Parse(parts[3].Replace("%", ""))),
            "P" => new PersonalComputer(id, name, isOn, parts.Length > 3 ? parts[3] : ""),
            "ED" => parts.Length < 5 ? throw new FormatException("Embedded device missing fields.") :
                new EmbeddedDevices(id, name, isOn, parts[3], parts[4]),
            _ => throw new FormatException($"Unknown device type: {type}")
        };
    }

    public void SaveToFile()
    {
        StreamWriter writer = new(filePath, false);
        foreach (var device in _devices)
            writer.WriteLine(device.ToString());
        writer.Close();
    }

    public void AddDevice(ElectronicDevice newDevice)
    {
        if (_devices.Count < MaxNumOfDevices && _devices.TrueForAll(d => d.Id != newDevice.Id))
        {
            _devices.Add(newDevice);
            Console.WriteLine($"Added device {newDevice.Name}-{newDevice.Id}");
            SaveToFile();
        }
            
    }

    public void RemoveDevice(int id)
    {
        if (_devices.RemoveAll(d => d.Id == id) > 0)
        {
            SaveToFile();
            Console.WriteLine($"Removed device {id}");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found");
        }
    }

    public void EditDevice(int id, string newName)
    {
        var device = _devices.Find(d => d.Id == id);
        if (device != null)
        {
            device.Name = newName; SaveToFile(); 
            Console.WriteLine($"No: {id} has cahnged its name {newName}");
        }
        Console.WriteLine($"Device with ID {id} not found");
    }
    
    public void UpdateBattery(int id, int newBattery)
    {
        var device = _devices.Find(d => d.Id == id && d is Smartwatches);
        if (device is Smartwatches watch)
        {
            watch.BatteryPercentage = newBattery;
            SaveToFile();
            Console.WriteLine($"Battery updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not a Smartwatch.");
        }
    }
    
    public void UpdateOperatingSystem(int id, string newOS)
    {
        var device = _devices.Find(d => d.Id == id && d is PersonalComputer);
        if (device is PersonalComputer pc)
        {
            pc.OperationSystem = newOS;
            SaveToFile();
            Console.WriteLine($"Operating System updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not a Personal Computer.");
        }
    }
    
    public void UpdateIpAddress(int id, string newIp)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        if (device is EmbeddedDevices ed)
        {
            ed.IpName = newIp;
            SaveToFile();
            Console.WriteLine($"IP Address updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void UpdateNetworkName(int id, string newNetwork)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        if (device is EmbeddedDevices ed)
        {
            ed.NetworkName = newNetwork;
            SaveToFile();
            Console.WriteLine($"Network Name updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void TurnOnDevice(int id)
    {
        ToggleDevice(id, true);
        Console.WriteLine($"Turned on device {id}");
    }

    public void TurnOffDevice(int id)
    {
        ToggleDevice(id, false);
        Console.WriteLine($"Turned off device {id}");
    }

    private void ToggleDevice(int id, bool state)
    {
        var device = _devices.Find(d => d.Id == id);
        if (device != null) { device.IsOn = state; SaveToFile(); }
    }

    public List<ElectronicDevice> GetAllDevices() => _devices;

    public void ShowAllDevices()
    {
        Console.WriteLine("All devices:");
        _devices.ForEach(Console.WriteLine);
        Console.WriteLine();
    }
}
