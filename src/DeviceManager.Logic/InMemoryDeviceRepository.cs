using DeviceManager.Entities;

namespace DeviceManager.Logic;

public class InMemoryDeviceRepository : IDeviceRepository
{
    private static readonly List<Device> Devices = new();

    public InMemoryDeviceRepository()
    {
        Devices.Add(new Smartwatches("SW-01", "Apple Smartwatch", true, 54));
        Devices.Add(new Smartwatches("SW-02", "Xiaomi watch", true, 94));
        Devices.Add(new PersonalComputer("P-01", "LinuxPC", false, "Linux Mint"));
        Devices.Add(new EmbeddedDevices("ED-01", "Weird Device", true, "123.53.14.0", "MD Ltd. Sound Services"));
    }

    public List<Device> LoadDevices()
    {
        return Devices;
    }

    public void SaveDevices(List<Device> devices)
    {
    }
}