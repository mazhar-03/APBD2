using DeviceManager.Entities;

namespace DeviceManager.Logic;

/// <summary>
///     Service class for managing devices that implements IDeviceManager.
/// </summary>
public class DeviceManagerService : IDeviceManager
{
    private const int MaxNumOfDevices = 15;
    private static List<Device> _devices = new();
    private readonly IDeviceRepository _deviceRepository;


    public DeviceManagerService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
        _devices = _deviceRepository.LoadDevices();
    }

    public void AddDevice(Device device)
    {
        if (_devices.Count >= MaxNumOfDevices)
            throw new ArgumentException("Device storage is full.");

        if (_devices.Any(d => d.Id == device.Id && d.GetType() == device.GetType()))
            throw new ArgumentException("Device already exists.");
        
        //improved turnOn validation for especially PC.
        var shouldBeOn = device.IsOn;
        device.IsOn = false;

        if (shouldBeOn)
            device.TurnOn(); 
        _devices.Add(device);
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

    public Device? GetDeviceById(string id)
    {
        return _devices.FirstOrDefault(d => d.Id == id);
    }
}