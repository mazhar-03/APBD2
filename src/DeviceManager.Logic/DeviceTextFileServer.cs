using DeviceManager.Entities;

namespace DeviceManager.Logic;

/// <summary>
///     Handles reading from and writing to the device data file.
/// </summary>
public class DeviceTextFileService : IDeviceRepository
{
    private readonly string _filePath;

    /// <summary>
    ///     Sets up the helper with the file path for device storage.
    /// </summary>
    /// <param name="filePath">The path to the device data file.</param>
    public DeviceTextFileService(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    ///     Loads all devices from the file.
    /// </summary>
    /// <returns>A list of devices loaded from the file.</returns>
    public List<Device> LoadDevices()
    {
        List<Device> devices = new();

        if (!File.Exists(_filePath))
        {
            Console.WriteLine("Device data file not found. Creating an empty list.");
            return devices;
        }

        var lines = File.ReadAllLines(_filePath);

        foreach (var line in lines)
            try
            {
                var device = DeviceParser.ParseDevice(line);
                if (device != null)
                {
                    devices.Add(device);
                    Console.WriteLine($"Loaded: {device}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
            }

        return devices;
    }

    /// <summary>
    ///     Saves all current devices to the file.
    /// </summary>
    /// <param name="devices">The list of devices to write.</param>
    public void SaveDevices(List<Device> devices)
    {
        var writer = new StreamWriter(_filePath, false);
        try
        {
            foreach (var device in devices) writer.WriteLine(device.ToString());
        }
        finally
        {
            writer.Close();
        }
    }
}