using DeviceManager.Entities;

namespace DeviceManager.Logic;

/// <summary>
///     Interface for managing a list of electronic devices.
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    ///     Adds a new device to the list.
    /// </summary>
    /// <param name="newDevice">The device to add.</param>
    void AddDevice(Device newDevice);

    /// <summary>
    ///     Removes a device by its ID and type.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="deviceType">The type of device (e.g., Smartwatches, PersonalComputer).</param>
    void RemoveDevice(string id, string deviceType);

    /// <summary>
    ///     Changes the name of a device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="deviceType">The type of device.</param>
    /// <param name="newName">The new name to give the device.</param>
    void EditDevice(string id, string deviceType, object newName);

    /// <summary>
    ///     Updates the battery level of a smartwatch.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="newBattery">The new battery level.</param>
    void UpdateBattery(string id, object newBattery);

    /// <summary>
    ///     Changes the operating system of a PC.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="newOs">The new operating system.</param>
    void UpdateOperatingSystem(string id, object newOs);

    /// <summary>
    ///     Changes the IP address of an embedded device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="newIp">The new IP address.</param>
    void UpdateIpAddress(string id, object newIp);

    /// <summary>
    ///     Changes the network name of an embedded device.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="newNetwork">The new network name.</param>
    void UpdateNetworkName(string id, object newNetwork);

    /// <summary>
    ///     Turns a device on.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="deviceType">The type of device.</param>
    void TurnOnDevice(string id, string deviceType);

    /// <summary>
    ///     Turns a device off.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="deviceType">The type of device.</param>
    void TurnOffDevice(string id, string deviceType);

    /// <summary>
    ///     Displays all devices in the list.
    /// </summary>
    void ShowAllDevices();

    /// <summary>
    ///     Gets the number of devices currently stored.
    /// </summary>
    /// <returns>The number of devices.</returns>
    int DeviceCount();

    /// <summary>
    ///     Returns all devices in the list.
    /// </summary>
    List<Device> GetAllDevices();

    Device? GetDeviceById(string id);
}