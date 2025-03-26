namespace Project;

public interface IDeviceRepository
{
    List<IDevice> LoadDevices();
    void SaveDevices(List<IDevice> devices);
}