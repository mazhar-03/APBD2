//
// ElectronicDevice dev1 = new ElectronicDevice(1, "Telephone", true);
// Console.WriteLine(dev1.ToString());

using System.Text.RegularExpressions;

public abstract class ElectronicDevice
{
    private int _id;
    private string _name;
    private bool _isOn;

    public int Id
    {
        get => _id;
        set
        {
            if (value < 0)
                throw new ArgumentException("Id cannot be negative");
            _id = value;
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if(value == null)
                throw new ArgumentException("Name cannot be null");
            _name = value;
        }
    }
    public bool IsOn => _isOn;
    
    public ElectronicDevice(int id, string name, bool isOn)
    {
        _id = id;
        _name = name;
        _isOn = isOn;
    }
    
    
    public virtual bool CanTurnOn()
    {
        return true; // Default implementation allows turning on
    }

    public virtual void TurnOn()
    {
        if(!CanTurnOn())
            throw new InvalidOperationException("Cannot turn on this device");
        _isOn = true;
    }
    
    public virtual void TurnOff()
    {
        _isOn = false;
    }
    
    public override string ToString()
    {
        return $"Device [ID: {Id}, Name: {Name}, Status: {(IsOn ? "On" : "Off")}]";
    }
}


public class Smartwatches : ElectronicDevice, IPowerNotifier
{
    private int _batteryPercentage;

    public int BatteryPercentage
    {
        get => _batteryPercentage;
        set
        {
            if(value < 0 || value > 100)
                throw new ArgumentException("Battery percentage must be between 0 and 100");
            _batteryPercentage = value;
            
            if(value < 20)
                Notify("Battery is low!!!!");    
        }
    }

    public Smartwatches(int id, string name, bool isOn, int batteryPercentage) : base(id, name, isOn)
    {
        _batteryPercentage = batteryPercentage;
    }

    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    public override bool CanTurnOn()
    {
        if(_batteryPercentage == 0 )
            throw new EmptyBatteryException("Smartwatches can not be turned on with no battery percentage");
        _batteryPercentage -= 10;
        return true;
    }

    public override string ToString()
    {
        return base.ToString() + $"Battery Percentage: {_batteryPercentage}";
    }
}

public interface IPowerNotifier
{
    void Notify(string notification);
}

public class EmptyBatteryException : Exception
{
    public EmptyBatteryException(string? message) : base(message) {}
}

public class PersonalComputer : ElectronicDevice
{
    private string _operationSystem;

    public string OperationSystem { get; set; }

    public override bool CanTurnOn()
    {
        if(string.IsNullOrEmpty(OperationSystem))
            throw new EmptySystemException("PC's without any operation system specified, cannot be launched");
        return true;
    }


    public PersonalComputer(int id, string name, bool isOn, string operationSystem) : base(id, name, isOn)
    {
        _operationSystem = operationSystem;
    }

    public override string ToString()
    {
        return base.ToString() + $"Operation System: {_operationSystem}";
    }
}

public class EmptySystemException : Exception
{
    public EmptySystemException(string? message) : base(message) {}
}

public class EmbeddedDevices : ElectronicDevice
{
    private string _ipAddress;
    public string NetworkName { get; set; }


    public string IpName
    {
        get => _ipAddress;
        set
        {
            //got it from stack overflow 
            // https://stackoverflow.com/questions/4890789/regex-for-an-ip-address
            Regex regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if(!regex.IsMatch(value))
                throw new ArgumentException("Invalid IP address specified");
            _ipAddress = value;
        }
    }

    public EmbeddedDevices(int id, string name, bool isOn, string ipAddress, string connectionName) : base(id, name, isOn)
    {
        _ipAddress = ipAddress;
        NetworkName = connectionName;
    }

    public void Connect()
    {
        if(!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException("Cannot connect to a device. Contains banned phrase");
    }

    public override bool CanTurnOn()
    {
        Connect(); 
        return true;
    }

    public override string ToString()
    {
        return base.ToString() + $"IP: {_ipAddress} - Network: {NetworkName}";
    }
}
public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message) { }
}

