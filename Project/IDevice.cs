namespace Project;

/// <summary>
/// Basic things every device should have and be able to do.
/// </summary>
public interface IDevice
{
    /// <summary>
    /// The unique ID of the device.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// The name of the device.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Whether the device is currently on.
    /// </summary>
    bool IsOn { get; set; }

    /// <summary>
    /// Turns the device on.
    /// </summary>
    void TurnOn();

    /// <summary>
    /// Turns the device off.
    /// </summary>
    void TurnOff();

    /// <summary>
    /// Returns basic info about the device as a string.
    /// </summary>
    /// <returns>Formatted device info.</returns>
    string ToString();
}
