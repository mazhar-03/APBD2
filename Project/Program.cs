using System.Text.RegularExpressions;

DeviceManager deviceManager = new DeviceManager("input.txt");
deviceManager.ShowAllDevices();

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

    public virtual void TurnOn()
    {
        IsOn = true;
    }

    public void TurnOff()
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
    private bool _notifiedLowBattery = false;  // Checking for it appears just once

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
                Notify("Smartwatch's battery is low!!!!");
                _notifiedLowBattery = true;  // 
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
        Console.WriteLine($"Smartwatch turned on. New battery: {BatteryPercentage}%");

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
    public PersonalComputer(int id, string name, bool isOn, string operationSystem)
        : base(id, name, isOn)
    {
        OperationSystem = operationSystem;

        // 🔹 Ensure that a PC cannot be created as "ON" without an OS
        if (isOn && string.IsNullOrWhiteSpace(operationSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    public string OperationSystem { get; set; }

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperationSystem))
            throw new EmptySystemException("PC cannot be launched without a proper operating system.");

        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | Operation System: {(string.IsNullOrWhiteSpace(OperationSystem) ? "No OS Installed" : OperationSystem)}";
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
            if (!value.Contains("MD Ltd."))
                throw new ConnectionException("Invalid network name! Embedded devices must connect to 'MD Ltd.'.");
            _networkName = value;
        }
    }

    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException("Cannot connect to a device. Must have MD Ltd.");
        Console.WriteLine($"Connecting to device: {NetworkName}");
    }

    public override void TurnOn()
    {
        try
        {
            Connect();
        }
        catch (ConnectionException ex)
        {
            Console.WriteLine($"{ex.Message}");
            return;
        }

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

    public DeviceManager(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist");


        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                Console.WriteLine($"Skipping invalid line: {line}\n");
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

    public void ShowAllDevices()
    {
        Console.WriteLine("\nDevices:");
        foreach (var device in _devices) Console.WriteLine(device);
        Console.WriteLine();
    }

    public void AddDevice(ElectronicDevice newDevice)
    {
        if (_devices.Any(d => d.Id == newDevice.Id))
        {
            Console.WriteLine($"A device with ID {newDevice.Id} already exists. Cannot add duplicate IDs.");
            return;
        }

        if (_devices.Count >= MaxNumOfDevices) return;

        _devices.Add(newDevice);
    }

    public void RemoveDevice(int id)
    {
        var deviceToRemove = _devices.FirstOrDefault(d => d.Id == id);
        if (deviceToRemove == null) return;
        _devices.Remove(deviceToRemove);
    }

    public void EditDevice(int id, string newName, object newData, object? newNetwork = null)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);

        if (device == null)
        {
            Console.WriteLine($"No device found with ID {id}. Cannot edit.");
            return;
        }

        try
        {
            // 🔹 Update name if a new name is provided
            if (!string.IsNullOrWhiteSpace(newName) && newName != device.Name)
                device.Name = newName;

            switch (device)
            {
                case Smartwatches sw when newData is int battery:
                    if (battery is < 0 or > 100)
                        throw new ArgumentException("Battery percentage must be between 0 and 100.");
                    sw.BatteryPercentage = battery;
                    break;

                case PersonalComputer pc when newData is string os:
                    if (string.IsNullOrWhiteSpace(os))
                        throw new ArgumentException("Operating system cannot be empty.");
                    pc.OperationSystem = os;
                    break;

                case EmbeddedDevices ed when newData is string ip:
                    ed.IpName = ip;
                    if (newNetwork is string network)
                        ed.NetworkName = network;
                    break;

                default:
                    throw new ArgumentException("Invalid device data.");
            }

            Console.WriteLine($"Device (ID: {id}) updated successfully!");
        }
        catch (InvalidCastException)
        {
            Console.WriteLine($"Data type mismatch while updating device (ID: {id}).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed for device (ID: {id}): {ex.Message}");
        }
    }

    public void TurnOn(int id)
    {
        var device = _devices.Find(d => d.Id == id);
        if (device == null) return;

        try
        {
            switch (device)
            {
                case Smartwatches sw:
                    if (sw.BatteryPercentage <= 10)
                        throw new EmptyBatteryException($"Smartwatch (ID: {id}) cannot turn on due to low battery.");
                    sw.BatteryPercentage -= 10;
                    break;

                case PersonalComputer pc:
                    if (string.IsNullOrWhiteSpace(pc.OperationSystem))
                        throw new EmptySystemException($"PC (ID: {id}) cannot turn on without an operating system.");
                    break;

                case EmbeddedDevices ed:
                    ed.Connect();
                    break;

                default:
                    throw new InvalidOperationException($"Unknown device type for ID {id}.");
            }

            device.TurnOn(); // Call base method for turning on
        }
        catch (Exception ex)
        {
        }
    }

    public void TurnOff(int id)
    {
        var device = _devices.Find(d => d.Id == id);

        if (device == null) return;

        device.TurnOff();
    }
}