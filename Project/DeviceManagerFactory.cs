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

    // Constructor receives the repository to use (can be injected)
    public DeviceManagerFactory(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    // Method to create and return an instance of DeviceManagerService
    public IDeviceManager CreateDeviceManager()
    {
        // You can add any additional initialization logic here if necessary
        return new DeviceManagerService(_deviceRepository);
    }
}