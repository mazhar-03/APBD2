namespace DeviceManager.Entities;

/// <summary>
///     A personal computer that can run an operating system.
/// </summary>
public class PersonalComputer : Device
{
    /// <summary>
    ///     Creates a new PC with its basic info and OS.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="name">The name of the PC.</param>
    /// <param name="isOn">Whether the PC is initially turned on.</param>
    /// <param name="operatingSystem">The operating system name.</param>
    /// <exception cref="EmptySystemException">Thrown if PC is turned on without an OS.</exception>
    public PersonalComputer(string id, string name, bool isOn, string? operatingSystem) : base(id, name, isOn)
    {
        OperatingSystem = operatingSystem;
        if (isOn && string.IsNullOrWhiteSpace(operatingSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    /// <summary>
    ///     The operating system installed on the PC.
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    ///     Turns the PC on. Throws if no OS is installed.
    /// </summary>
    /// <exception cref="EmptySystemException">Thrown if no OS is set.</exception>
    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
            throw new EmptySystemException("PC cannot be launched without an operating system.");
        base.TurnOn();
    }

    /// <summary>
    ///     Returns PC details in the file format.
    /// </summary>
    public override string ToString()
    {
        return $"P-{Id},{Name},{IsOn},{OperatingSystem}";
    }
}