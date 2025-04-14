using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DeviceManager.Entities;

/// <summary>
///     Represents an embedded device that connects to a network and has an IP address.
/// </summary>
public class EmbeddedDevices : Device
{
    private string _ipAddress;
    private string _networkName;

    /// <summary>
    ///     Creates a new embedded device with IP and network name.
    /// </summary>
    /// <param name="id">The device ID.</param>
    /// <param name="name">The device name.</param>
    /// <param name="isOn">Whether the device is on initially.</param>
    /// <param name="ipName">The IP address (matches IpName property).</param>
    /// <param name="networkName">The network name (matches NetworkName property).</param>
    [JsonConstructor]
    public EmbeddedDevices(string id, string name, bool isOn, string ipName, string networkName)
        : base(id, name, isOn)
    {
        IpName = ipName;
        NetworkName = networkName;
    }

    /// <summary>
    ///     The IP address of the device. Must be in valid IPv4 format.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the IP format is invalid.</exception>
    public string IpName
    {
        get => _ipAddress;
        set
        {
            if (!Regex.IsMatch(value, @"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                      @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                      @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\." +
                                      @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$"))
                throw new ArgumentException("Invalid IP address specified");
            _ipAddress = value;
        }
    }

    /// <summary>
    ///     The network name the device is connected to. Must contain 'MD Ltd.'.
    /// </summary>
    /// <exception cref="ConnectionException">Thrown if the network name is not allowed.</exception>
    public string NetworkName
    {
        get => _networkName;
        set
        {
            if (!value.Contains("MD Ltd."))
                throw new ConnectionException("Invalid network name! Must connect to 'MD Ltd.'");
            _networkName = value;
        }
    }

    /// <summary>
    ///     Returns all device info as a string formatted for file storage.
    /// </summary>
    public override string ToString()
    {
        return $"ED-{Id},{Name},{IsOn},{IpName},{NetworkName}";
    }
}