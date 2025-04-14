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
        var deviceToRemove = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (deviceToRemove != null)
        {
            _devices.Remove(deviceToRemove);
            _deviceRepository.SaveDevices(_devices);
        }
    }

    public void EditDevice(string id, string deviceType, object newName)
    {
        var device = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (device == null)
            throw new ArgumentException("Device not found.");

        var name = newName as string;
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name must be a non-empty string.");

        device.Name = name.Trim();
        _deviceRepository.SaveDevices(_devices);
    }


    public void UpdateBattery(string id, object newBattery)
    {
        var device = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) as Smartwatches;
        if (device == null)
            throw new ArgumentException("Device is not a Smartwatch.");

        if (newBattery is not int level)
            throw new ArgumentException("Battery must be an integer.");

        device.BatteryPercentage = level;
        _deviceRepository.SaveDevices(_devices);
    }


    public void UpdateOperatingSystem(string id, object newOs)
    {
        var device =
            _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) as PersonalComputer;
        if (device == null)
            throw new ArgumentException("Device is not a PersonalComputer.");

        var os = newOs as string;
        if (string.IsNullOrWhiteSpace(os))
            throw new ArgumentException("Operating system name cannot be empty.");

        device.OperatingSystem = os.Trim();
        _deviceRepository.SaveDevices(_devices);
    }

    public void UpdateIpAddress(string id, object newIp)
    {
        var device =
            _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) as EmbeddedDevices;
        if (device == null)
            throw new ArgumentException("Device is not an EmbeddedDevice.");

        var ip = newIp as string;
        if (string.IsNullOrWhiteSpace(ip))
            throw new ArgumentException("Invalid or null IP address.");

        device.IpName = ip;
        _deviceRepository.SaveDevices(_devices);
    }

    public void UpdateNetworkName(string id, object newNetwork)
    {
        var device =
            _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) as EmbeddedDevices;
        if (device == null)
            throw new ArgumentException("Device is not an EmbeddedDevice.");

        var network = newNetwork as string;
        if (string.IsNullOrWhiteSpace(network))
            throw new ArgumentException("Network name must be a valid string.");

        device.NetworkName = network.Trim();
        _deviceRepository.SaveDevices(_devices);
    }

    public void TurnOnDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (device != null)
        {
            device.TurnOn();
            _deviceRepository.SaveDevices(_devices);
        }
    }

    public void TurnOffDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        ;
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
        return _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}