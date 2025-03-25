namespace Project;

public interface IDeviceService
{
    /// <summary>
    ///     Interface for managing devices with higher-level operations.
    /// </summary>
    void AddDevice(IDevice newDevice);

    void RemoveDevice(string id);
    void EditDeviceName(string id, string newName);
    void UpdateDeviceBattery(string id, object newBattery);
    void UpdateDeviceOperatingSystem(string id, object newOs);
    void UpdateDeviceIpAddress(string id, object newIp);
    void UpdateDeviceNetworkName(string id, object newNetwork);
    void TurnOnDevice(string id);
    void TurnOffDevice(string id);
    void ShowAllDevices();
    int DeviceCount();
}