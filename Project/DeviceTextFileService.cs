namespace Project;

/// <summary>
/// Handles reading from and writing to the device data file.
/// </summary>
public class DeviceTextFileService
{
    private string _filePath;

    /// <summary>
    /// Sets up the helper with the file path for device storage.
    /// </summary>
    /// <param name="filePath">The path to the device data file.</param>
    public DeviceTextFileService(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Loads all devices from the file.
    /// </summary>
    /// <returns>A list of devices loaded from the file.</returns>
    public List<IDevice> LoadDevices()
    {
        List<IDevice> devices = new();

        if (!File.Exists(_filePath))
        {
            Console.WriteLine("Device data file not found. Creating an empty list.");
            return devices;
        }

        var lines = File.ReadAllLines(_filePath);

        foreach (var line in lines)
            try
            {
                var device = ParseDevice(line);
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
    /// Saves all current devices to the file.
    /// </summary>
    /// <param name="devices">The list of devices to write.</param>
    public void SaveDevices(List<IDevice> devices)
    {
        using StreamWriter writer = new(_filePath, false);
        foreach (var device in devices)
            writer.WriteLine(device.ToString());
    }

    /// <summary>
    /// Parses one line of text into a device.
    /// </summary>
    /// <param name="line">The text line to parse.</param>
    /// <returns>The parsed device, or throws if the format is wrong.</returns>
    /// <exception cref="FormatException">Thrown if the line is not correctly formatted.</exception>
    private IDevice ParseDevice(string line)
    {
        string[] parts = line.Split(',');

        if (parts.Length < 3)
            throw new FormatException("Incomplete device data.");

        var typeAndId = parts[0].Split('-');
        if (typeAndId.Length < 2)
            throw new FormatException("Invalid device type format.");

        var type = typeAndId[0].Trim();
        var id = typeAndId[1].Trim();
        var name = parts[1].Trim();
        var isOn = parts[2].Trim().ToLower() == "true";

        return type switch
        {
            "SW" => new Smartwatches(id, name, isOn, int.Parse(parts[3].Replace("%", "").Trim())),
            "P" => new PersonalComputer(id, name, isOn, parts.Length >= 4 ? parts[3].Trim() : null),
            "ED" => new EmbeddedDevices(id, name, isOn, parts[3], parts[4]),
            _ => throw new FormatException($"Unknown device type: {type}")
        };
    }
}