using DeviceManager.Entities;

namespace DeviceManager.Logic;

public class InMemoryDeviceRepository : IDeviceRepository
{
    private static readonly List<Device> Devices = new();

    public List<Device> LoadDevices()
    {
        return Devices;
    }

    public void SaveDevices(List<Device> devices)
    {
    }
}