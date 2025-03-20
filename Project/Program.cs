using System.Text.RegularExpressions;

var manager = new DeviceManager("devices.txt");
manager.AddDevice(new EmbeddedDevices(2, "EDDDD", true , "123.243.151.31", "MD Ltd. home"));


public abstract class ElectronicDevice
{
    public ElectronicDevice(int id, string name, bool isOn)
    {
        Id = id < 0 ? throw new ArgumentException("Id cannot be negative") : id;
        Name = name ?? throw new ArgumentException("Name cannot be null");
        IsOn = isOn;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }

    public virtual void TurnOn()
    {
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

    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
            throw new EmptyBatteryException("Smartwatches cannot turn on with battery less than 11%");
        BatteryPercentage -= 10;
        base.TurnOn();
    }
    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"SW-{Id},{Name},{IsOn},{BatteryPercentage}%";
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
    public PersonalComputer(int id, string name, bool isOn, string operatingSystem) : base(id, name, isOn)
    {
        OperatingSystem = operatingSystem;
        if (isOn && string.IsNullOrWhiteSpace(operatingSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    public string OperatingSystem { get; set; }

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
            throw new EmptySystemException("PC cannot be launched without an operating system.");
        base.TurnOn();
    }

    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"P-{Id},{Name},{IsOn},{OperatingSystem}";
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
    
    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"ED-{Id},{Name},{IsOn},{IpName},{NetworkName}";
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
    private string _filePath;
    private List<ElectronicDevice> _devices;
    private const int MaxNumOfDevices = 15;

    public DeviceManager(string filePath)
    {
        _filePath = filePath;
        _devices = new List<ElectronicDevice>();
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine("Device data file not found. Creating an empty list.");
            return;
        }

        string[] lines = File.ReadAllLines(_filePath);

        foreach (string line in lines)
        {
            try
            {
                var device = ParseDevice(line);
                if (device != null)
                {
                    _devices.Add(device);
                    Console.WriteLine($"Loaded: {device}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
            }
        }
    }

    private ElectronicDevice ParseDevice(string line)
    {
        string[] parts = line.Split(',');

        if (parts.Length < 3)
            throw new FormatException("Incomplete device data.");

        var typeAndId = parts[0].Split('-');
        if (typeAndId.Length < 2)
            throw new FormatException("Invalid device type format.");

        var type = typeAndId[0].Trim();
        var id = Convert.ToInt32(typeAndId[1].Trim());
        var name = parts[1].Trim();
        var isOn = parts[2].Trim().ToLower() == "true";

        if (type == "SW")
        {
            if (parts.Length < 4) throw new FormatException("Smartwatch missing battery percentage.");
            int battery = Convert.ToInt32(parts[3].Replace("%", "").Trim());
            return new Smartwatches(id, name, isOn, battery);
        }

        if (type == "P")
        {
            string os = parts.Length > 3 ? parts[3].Trim() : null;
            return new PersonalComputer(id, name, isOn, os);
        }

        if (type == "ED")
        {
            if (parts.Length < 5) throw new FormatException("Embedded device missing fields.");
            return new EmbeddedDevices(id, name, isOn, parts[3], parts[4]);
        }

        throw new FormatException($"Unknown device type: {type}");
    }

    public void SaveToFile()
    {
        StreamWriter writer = new(_filePath, false);
        foreach (var device in _devices)
            writer.WriteLine(device.ToString());
        writer.Close();
    }

    public void AddDevice(ElectronicDevice newDevice)
    {
        if (_devices.Count < MaxNumOfDevices &&
            !_devices.Any(d => d.Id == newDevice.Id && d.GetType() == newDevice.GetType()))
        {
            _devices.Add(newDevice);
            Console.WriteLine($"Added device {newDevice.Name}-{newDevice.Id}");
            SaveToFile();
        }
        else
        {
            Console.WriteLine(
                $"Cannot add device {newDevice.Name}-{newDevice.Id}: Duplicate ID for the same type exists.");
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

    public void EditDevice(int id, object newName)
    {
        var device = _devices.Find(d => d.Id == id);
        string newNewName = (string)newName;
        if (device != null)
        {
            device.Name = newNewName;
            SaveToFile();
            Console.WriteLine($"No: {id} has cahnged its name {newName}");
        }

        Console.WriteLine($"Device with ID {id} not found");
    }

    public void UpdateBattery(int id, object newBattery)
    {
        var device = _devices.Find(d => d.Id == id && d is Smartwatches);
        int newBatteryString = (int)newBattery;
        if (device is Smartwatches watch)
        {
            watch.BatteryPercentage = newBatteryString;
            SaveToFile();
            Console.WriteLine($"Battery updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not a Smartwatch.");
        }
    }

    public void UpdateOperatingSystem(int id, object newOs)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is PersonalComputer);
        if (device == null)
        {
            Console.WriteLine($"Device with ID {id} not found or not a Personal Computer.");
            return;
        }

        string osString = (string)newOs;

        ((PersonalComputer)device).OperatingSystem = osString;
        Console.WriteLine($"Updated OS for device ID {id} to {osString}");
        SaveToFile();
    }

    public void UpdateIpAddress(int id, object newIp)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        string ipString = (string)newIp;
        if (device is EmbeddedDevices ed)
        {
            ed.IpName = ipString;
            SaveToFile();
            Console.WriteLine($"IP Address updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void UpdateNetworkName(int id, object newNetwork)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        string networkString = (string)newNetwork;
        if (device is EmbeddedDevices ed)
        {
            ed.NetworkName = networkString;
            SaveToFile();
            Console.WriteLine($"Network Name updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void TurnOnDevice(int id, string deviceType)
    {
        var device = _devices.Find(d => d.Id == id && d.GetType().Name.Equals(deviceType, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            Console.WriteLine($"Device with ID {id} and type {deviceType} not found.");
            return;
        }

        try
        {
            device.TurnOn();
            SaveToFile();
            Console.WriteLine($"Device with ID {device.Id} ({deviceType}) is now ON.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error turning on device: {ex.Message}");
        }
    }

    public void TurnOffDevice(int id, string deviceType)
    {
        var device = _devices.Find(d => d.Id == id && d.GetType().Name.Equals(deviceType, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            Console.WriteLine($"Device with ID {id} and type {deviceType} not found.");
            return;
        }

        try
        {
            device.TurnOff();
            SaveToFile();
            Console.WriteLine($"Device with ID {device.Id} ({deviceType}) is now OFF.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error turning of device: {ex.Message}");
        }    
    }
    public int DeviceCount => _devices.Count;
    
    public void ShowAllDevices()
    {
        Console.WriteLine("All devices:");
        _devices.ForEach(Console.WriteLine);
        Console.WriteLine();
    }
}