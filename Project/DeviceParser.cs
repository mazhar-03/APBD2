namespace Project;

public static class DeviceParser
{
    /// <summary>
    ///     Parses one line of text into a device.
    /// </summary>
    /// <param name="line">The text line to parse.</param>
    /// <returns>The parsed device, or throws if the format is wrong.</returns>
    /// <exception cref="FormatException">Thrown if the line is not correctly formatted.</exception>
    public static Device ParseDevice(string line)
    {
        var parts = line.Split(',');

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
            "P" => new PersonalComputer(id, name, isOn, parts.Length >= 3 ? parts[3].Trim() : null),
            "ED" => new EmbeddedDevices(id, name, isOn, parts[3], parts[4]),
            _ => throw new FormatException($"Unknown device type: {type}")
        };
    }
}