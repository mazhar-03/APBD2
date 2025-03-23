namespace Project;

public static class DeviceManagerFactory
{
    public static IDeviceManager Create(string filePath)
    {
        return new DeviceManager(filePath);
    }
}
