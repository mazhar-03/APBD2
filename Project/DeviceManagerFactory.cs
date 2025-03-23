namespace Project;

public static class DeviceManagerFactory
{
    public static DeviceManager Create(string filePath)
    {
        return new DeviceManager(filePath);
    }
}