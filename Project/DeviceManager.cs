namespace Project;

public class DeviceManager : IDeviceManager
{
    private const int MaxNumOfDevices = 15;
    private List<IDevice> _devices;
    private DeviceManagerHelper _helper;

    public DeviceManager(string filePath)
    {
        _helper = new DeviceManagerHelper(filePath);
        _devices = _helper.LoadDevices();
    }
    
    private void Save()
    {
        _helper.SaveDevices(_devices);
    }

    public void AddDevice(IDevice newDevice)
    {
        if (_devices.Count < MaxNumOfDevices &&
            !_devices.Any(d => d.Id == newDevice.Id && d.GetType() == newDevice.GetType()))
        {
            _devices.Add(newDevice);
            Console.WriteLine($"Added device {newDevice.Name}-{newDevice.Id}");
            Save();
        }
        else
        {
            Console.WriteLine(
                $"Cannot add device {newDevice.Name}-{newDevice.Id}: Duplicate ID for the same type exists.");
        }
    }

    public void RemoveDevice(string id, string deviceType)
    {
        var removedCount = _devices.RemoveAll(d =>
            d.Id == id && d.GetType().Name.Equals(deviceType, StringComparison.OrdinalIgnoreCase));

        if (removedCount > 0)
        {
            Save();
            Console.WriteLine($"Removed {deviceType} with ID {id}");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} and type {deviceType} not found");
        }
    }

    public void EditDevice(string id, string deviceType ,object newName)
    {
        var device = FindDevice(id, deviceType);
        var newNewName = (string)newName;

        if (device != null)
        {
            device.Name = newNewName;
            Save();
            Console.WriteLine($"No: {id}-{deviceType} has changed its name to {newName}");
        }
        else
        {
            Console.WriteLine($"Device with ID {id}-{deviceType} not found");
        }
    }


    public void UpdateBattery(string id, object newBattery)
    {
        var device = _devices.Find(d => d.Id == id && d is Smartwatches);
        var newBatteryString = (int)newBattery;
        if (device is Smartwatches watch)
        {
            watch.BatteryPercentage = newBatteryString;
            Save();
            Console.WriteLine($"Battery updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not a Smartwatch.");
        }
    }

    public void UpdateOperatingSystem(string id, object newOs)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is PersonalComputer);
        if (device == null)
        {
            Console.WriteLine($"Device with ID {id} not found or not a Personal Computer.");
            return;
        }

        var osString = (string)newOs;

        ((PersonalComputer)device).OperatingSystem = osString;
        Console.WriteLine($"Updated OS for device ID {id} to {osString}");
        Save();
    }

    public void UpdateIpAddress(string id, object newIp)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        var ipString = (string)newIp;
        if (device is EmbeddedDevices ed)
        {
            ed.IpName = ipString;
            Save();
            Console.WriteLine($"IP Address updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void UpdateNetworkName(string id, object newNetwork)
    {
        var device = _devices.Find(d => d.Id == id && d is EmbeddedDevices);
        var networkString = (string)newNetwork;
        if (device is EmbeddedDevices ed)
        {
            ed.NetworkName = networkString;
            Save();
            Console.WriteLine($"Network Name updated for device ID {id}.");
        }
        else
        {
            Console.WriteLine($"Device with ID {id} not found or not an Embedded Device.");
        }
    }

    public void TurnOnDevice(string id, string deviceType)
    {
        var device = FindDevice(id, deviceType);

        if (device == null)
        {
            Console.WriteLine($"Device with ID '{id}' and type '{deviceType}' not found.");
            return;
        }

        try
        {
            if (device.IsOn)
            {
                Console.WriteLine($"Device {id}-{deviceType} is already turned on.");
                return;
            }

            if (device is PersonalComputer pc && string.IsNullOrWhiteSpace(pc.OperatingSystem))
                throw new EmptySystemException($"{pc.Name} cannot be launched without an operating system.");
            device.TurnOn();
            Save();
            Console.WriteLine($"Device '{id}' ({deviceType}) is now ON.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error turning on device '{id}': {ex.Message}");
            throw;
        }
    }


    public void TurnOffDevice(string id, string deviceType)
    {
        var device = FindDevice(id, deviceType);

        if (device == null)
        {
            Console.WriteLine($"Device with ID '{id}' and type '{deviceType}' not found.");
            return;
        }

        try
        {
            if (!device.IsOn)
            {
                Console.WriteLine($"Device {id}-{deviceType} is already turned off.");
                return;
            }

            device.TurnOff();
            Save();
            Console.WriteLine($"Device '{id}' ({deviceType}) is now OFF.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error turning off device '{id}': {ex.Message}");
        }
    }

    public void ShowAllDevices()
    {
        Console.WriteLine("All devices:");
        _devices.ForEach(Console.WriteLine);
        Console.WriteLine();
    }

    private IDevice FindDevice(string id, string deviceType)
    {
        return _devices.Find(d =>
            d.Id == id && d.GetType().Name.Equals(deviceType, StringComparison.OrdinalIgnoreCase));
    }

    public int DeviceCount()
    {
        return _devices.Count;    
    } 
}