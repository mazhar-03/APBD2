namespace Project;

public interface IDeviceRepository
{
    List<Device> LoadDevices();
    void SaveDevices(List<Device> devices);
}