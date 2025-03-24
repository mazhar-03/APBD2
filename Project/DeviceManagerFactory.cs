namespace Project;

/// <summary>
/// Helps create a device manager instance.
/// </summary>
public static class DeviceManagerFactory
{
    /// <summary>
    /// Creates a new device manager with a given file path.
    /// </summary>
    /// <param name="filePath">The path to the file used for loading/saving devices.</param>
    /// <returns>An instance of IDeviceManager.</returns>
    public static IDeviceManager Create(string filePath)
    {
        return new DeviceManager(filePath);
    }
}
