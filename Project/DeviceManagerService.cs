namespace Project;

/// <summary>
///     Service class for managing devices that implements IDeviceManager.
/// </summary>
public class DeviceManagerService : IDeviceManager
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly List<IDevice> _devices;

    // Constructor that receives IDeviceRepository (Dependency Injection)
    public DeviceManagerService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
        _devices = _deviceRepository.LoadDevices(); // Load devices at initialization
    }

    // Add a new device
    public void AddDevice(IDevice newDevice)
    {
        _devices.Add(newDevice);
        _deviceRepository.SaveDevices(_devices); // Save after adding
    }

    // Remove a device by its ID and type
    public void RemoveDevice(string id, string deviceType)
    {
        var deviceToRemove = _devices.FirstOrDefault(d => d.Id == id);
        if (deviceToRemove != null)
        {
            _devices.Remove(deviceToRemove);
            _deviceRepository.SaveDevices(_devices); // Save after removing
        }
    }

    // Edit the name of a device
    public void EditDevice(string id, string deviceType, object newName)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.Name = newName.ToString();
            _deviceRepository.SaveDevices(_devices); // Save after editing
        }
    }

    // Update battery level of a smartwatch
    public void UpdateBattery(string id, object newBattery)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is Smartwatches);
        if (device != null)
        {
            ((Smartwatches)device).BatteryPercentage = (int)newBattery;
            _deviceRepository.SaveDevices(_devices); // Save after updating battery
        }
    }

    // Update operating system of a PC
    public void UpdateOperatingSystem(string id, object newOs)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is PersonalComputer);
        if (device != null)
        {
            ((PersonalComputer)device).OperatingSystem = newOs.ToString();
            _deviceRepository.SaveDevices(_devices); // Save after updating OS
        }
    }

    // Update IP address of an embedded device
    public void UpdateIpAddress(string id, object newIp)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is EmbeddedDevices);
        if (device != null)
        {
            ((EmbeddedDevices)device).IpName = newIp.ToString();
            _deviceRepository.SaveDevices(_devices); // Save after updating IP address
        }
    }

    // Update network name of an embedded device
    public void UpdateNetworkName(string id, object newNetwork)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id && d is EmbeddedDevices);
        if (device != null)
        {
            ((EmbeddedDevices)device).NetworkName = newNetwork.ToString();
            _deviceRepository.SaveDevices(_devices); // Save after updating network name
        }
    }

    // Turn on a device
    public void TurnOnDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.TurnOn();
            _deviceRepository.SaveDevices(_devices); // Save after turning on
        }
    }

    // Turn off a device
    public void TurnOffDevice(string id, string deviceType)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            device.TurnOff();
            _deviceRepository.SaveDevices(_devices); // Save after turning off
        }
    }

    // Show all devices
    public void ShowAllDevices()
    {
        foreach (var device in _devices) Console.WriteLine(device.ToString());
    }

    // Get the count of devices
    public int DeviceCount()
    {
        return _devices.Count;
    }
}