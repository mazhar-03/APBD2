namespace Project;

/// <summary>
/// Base class for all electronic devices.
/// </summary>
public abstract class ElectronicDevice : IDevice
{
    /// <summary>
    /// Sets the basic info of a device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="name">The device name.</param>
    /// <param name="isOn">Whether the device is turned on initially.</param>
    protected ElectronicDevice(string id, string name, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty");
        Id = id;
        Name = name ?? throw new ArgumentException("Name cannot be null");
        IsOn = isOn;
    }

    /// <summary>
    /// The ID of the device.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the device.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Whether the device is currently on.
    /// </summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Turns the device on. Throws if it's already on.
    /// </summary>
    public virtual void TurnOn()
    {
        if (IsOn)
            throw new InvalidOperationException("Device is already turned on.");
        IsOn = true;
    }

    /// <summary>
    /// Turns the device off. Throws if it's already off.
    /// </summary>
    public virtual void TurnOff()
    {
        if (!IsOn)
            throw new InvalidOperationException("Device is already turned off.");
        IsOn = false;
    }

    /// <summary>
    /// Returns basic info about the device as a string.
    /// </summary>
    public override string ToString()
    {
        return $"Device [ID: {Id}, Name: {Name}, Status: {(IsOn ? "On" : "Off")}]";
    }
}