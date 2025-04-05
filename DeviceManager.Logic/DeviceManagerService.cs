using DeviceManager.Entities;

namespace DeviceManager.Logic
{
    /// <summary>
///     Service class for managing devices that implements IDeviceManager.
/// </summary>
public class DeviceManagerService : IDeviceManager
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly List<Device> _devices;


    public DeviceManagerService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
        _devices = _deviceRepository.LoadDevices(); 
    }

    private const int MaxNumOfDevices = 15;

    public void AddDevice(Device newDevice)
    {
        if (_devices.Count < MaxNumOfDevices &&
            !_devices.Any(d => d.Id == newDevice.Id && d.GetType() == newDevice.GetType()))
        {
            _devices.Add(newDevice);
            Console.WriteLine($"Added device {newDevice.Name}-{newDevice.Id}");
            _deviceRepository.SaveDevices(_devices); 
        }
        else
        {
            Console.WriteLine(
                $"Cannot add device {newDevice.Name}-{newDevice.Id}: Duplicate ID for the same type exists.");
        }
    }

    public void RemoveDevice(string id, string deviceType)
    {
        var deviceToRemove = _devices.FirstOrDefault(d => d.Id == id);
        if (deviceToRemove != null)
        {
            _devices.Remove(deviceToRemove);
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    public void EditDevice(string id, string deviceType, object newName)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.Name = newName.ToString() ?? throw new InvalidOperationException();
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    public void UpdateBattery(string id, object newBattery)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is Smartwatches);
        if (device != null)
        {
            ((Smartwatches)device).BatteryPercentage = (int)newBattery;
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    public void UpdateOperatingSystem(string id, object newOs)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is PersonalComputer);
        if (device != null)
        {
            ((PersonalComputer)device).OperatingSystem = newOs.ToString();
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    public void UpdateIpAddress(string id, object newIp)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is EmbeddedDevices);
        if (device != null)
        {
            ((EmbeddedDevices)device).IpName = newIp.ToString() ?? throw new InvalidOperationException();
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    public void UpdateNetworkName(string id, object newNetwork)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is EmbeddedDevices);
        if (device != null)
        {
            ((EmbeddedDevices)device).NetworkName = newNetwork.ToString() ?? throw new InvalidOperationException();
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    // Turn on a device
    public void TurnOnDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.TurnOn();
            _deviceRepository.SaveDevices(_devices); 
        }
    }

    // Turn off a device
    public void TurnOffDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.TurnOff();
            _deviceRepository.SaveDevices(_devices);
        }
    }

    public void ShowAllDevices()
    {
        foreach (var device in _devices) Console.WriteLine(device.ToString());
    }

    public int DeviceCount()
    {
        return _devices.Count;
    }

    public List<Device> GetAllDevices()
    {
        return _devices;
    }
}
}
