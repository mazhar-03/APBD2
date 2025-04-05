using DeviceManager.Entities;

namespace DeviceManager.Logic
{
    public interface IDeviceRepository
    {
        List<Device> LoadDevices();
        void SaveDevices(List<Device> devices);
    }
}


