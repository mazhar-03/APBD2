using System.Text.RegularExpressions;

namespace Project;

public class EmbeddedDevices : ElectronicDevice
{
    private string _ipAddress;
    private string _networkName;

    public EmbeddedDevices(string id, string name, bool isOn, string ipAddress, string connectionName) : base(id, name,
        isOn)
    {
        IpName = ipAddress;
        NetworkName = connectionName;
    }

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

    public string NetworkName
    {
        get => _networkName;
        set
        {
            if (!value.Contains("MD Ltd."))
                throw new ConnectionException("Invalid network name! Must connect to 'MD Ltd.'.");
            _networkName = value;
        }
    }

    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"ED-{Id},{Name},{IsOn},{IpName},{NetworkName}";
    }
}