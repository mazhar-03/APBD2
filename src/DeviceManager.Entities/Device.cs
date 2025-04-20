namespace DeviceManager.Entities;

/// <summary>
///     Abstract base class for all electronic devices.
/// </summary>
public abstract class Device
{
    /// <summary>
    ///     Sets the base properties of a device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="name">The device name.</param>
    /// <param name="isOn">The initial power state.</param>
    protected Device(string id, string name, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.");

        Id = id;
        Name = name;
        IsOn = isOn;
    }

    /// <summary>
    ///     The unique ID of the device.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the device.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Whether the device is currently on.
    /// </summary>
    public bool IsOn { get; set; }

    /// <summary>
    ///     Turns the device on.
    /// </summary>
    public virtual void TurnOn()
    {
        if (IsOn)
            throw new InvalidOperationException("Device is already turned on.");
        IsOn = true;
    }

    /// <summary>
    ///     Turns the device off.
    /// </summary>
    public virtual void TurnOff()
    {
        if (!IsOn)
            throw new InvalidOperationException("Device is already turned off.");
        IsOn = false;
    }

    /// <summary>
    ///     Returns the basic device info as a formatted string.
    /// </summary>
    public override string ToString()
    {
        return $"{Id},{Name},{IsOn}";
    }
}