namespace Project;

/// <summary>
///     Helps create a device manager instance.
/// </summary>
public class DeviceManagerFactory
{
    /// <summary>
    ///     Creates a new device manager with a given file path.
    /// </summary>
    /// <param name="filePath">The path to the file used for loading/saving devices.</param>
    /// <returns>An instance of IDeviceManager.</returns>
    private readonly IDeviceRepository _deviceRepository;

    public DeviceManagerFactory(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public IDeviceManager CreateDeviceManager()
    {
        return new DeviceManagerService(_deviceRepository);
    }
}