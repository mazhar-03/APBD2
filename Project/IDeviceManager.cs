namespace Project;

public interface IDeviceManager
{
    void AddDevice(IDevice newDevice);
    void RemoveDevice(string id, string deviceType);
    void EditDevice(string id, string deviceType,object newName);
    void UpdateBattery(string id, object newBattery);
    void UpdateOperatingSystem(string id, object newOs);
    void UpdateIpAddress(string id, object newIp);
    void UpdateNetworkName(string id, object newNetwork);
    void TurnOnDevice(string id, string deviceType);
    void TurnOffDevice(string id, string deviceType);
    void ShowAllDevices();
}