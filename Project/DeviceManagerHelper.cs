namespace Project;

public class DeviceManagerHelper
{
    private readonly string _filePath;

    public DeviceManagerHelper(string filePath)
    {
        _filePath = filePath;
    }

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

    public void SaveDevices(List<IDevice> devices)
    {
        using StreamWriter writer = new(_filePath, false);
        foreach (var device in devices)
            writer.WriteLine(device.ToString());
    }

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