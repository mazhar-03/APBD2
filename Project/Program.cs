using System.Diagnostics;
using System.Text.RegularExpressions;

//tests
/*
// Smartwatches smartwatches = new Smartwatches(1, "apple se", true, 45);

// PersonalComputer pc1 = new PersonalComputer(2, "Test PC", false, "");
// Console.WriteLine(pc1);
// pc1.TurnOn();
// Console.WriteLine(pc1);

// PersonalComputer pc2 = new PersonalComputer(3, "Old PC", false, "    ");
// Console.WriteLine(pc2);
// pc2.TurnOn();
// Console.WriteLine(pc2);

// PersonalComputer pc4 = new PersonalComputer(4, "Office PC", true, "");
// Console.WriteLine(pc4);

// EmbeddedDevices ed1 = new EmbeddedDevices(1, "Sensor", false, "192.168.1.1", "MD Ltd. Network");
// Console.WriteLine(ed1);
// ed1.TurnOn();
// Console.WriteLine(ed1);

// EmbeddedDevices ed2 = new EmbeddedDevices(2, "Camera", false, "999.999.999.999", "MD Ltd. Wifi");

// EmbeddedDevices ed3 = new EmbeddedDevices(3, "Router", false, "192.168.1.100", "Home Network");
// ed3.TurnOn();
*/

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
    
    
    protected virtual bool CanTurnOn()
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
            
            if(_batteryPercentage < 20)
                Notify("Battery is low!!!!");    
        }
    }

    public Smartwatches(int id, string name, bool isOn, int batteryPercentage) : base(id, name, isOn)
    {
        BatteryPercentage = batteryPercentage;
    }

    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    protected override bool CanTurnOn()
    {
        return _batteryPercentage > 0;
    }

    public override void TurnOn()
    {
        if(!CanTurnOn())
            throw new EmptyBatteryException("Smartwatches cannot turn on with no battery");
        
        //Since the reqs are say "for setting" we do not use BatteryPercentage
        _batteryPercentage -= 10;
        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | Battery Percentage: {BatteryPercentage}%";
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

    public string OperationSystem
    {
        get => _operationSystem;
        set
        {
            _operationSystem = value;
        }
    }

    public PersonalComputer(int id, string name, bool isOn, string operationSystem) : base(id, name, isOn)
    {
        OperationSystem = operationSystem;
        
        if (isOn && string.IsNullOrWhiteSpace(operationSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }
    
    //PC can not turn on without an OS
    protected override bool CanTurnOn()
    {
        return !string.IsNullOrWhiteSpace(OperationSystem);
    }
    
    //if user try to launch without an OS send our custom exception    
    public override void TurnOn()
    {
        if (!CanTurnOn())
            throw new EmptySystemException("PC can not be launched without an any proper operation system"); 
        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | Operation System: {_operationSystem}";
    }
}

public class EmptySystemException : Exception
{
    public EmptySystemException(string? message) : base(message) {}
}

public class EmbeddedDevices : ElectronicDevice
{
    private string _ipAddress;
    private string _networkName;
    
    public string IpName
    {
        get => _ipAddress;
        set
        {
            // got the regex values from gpt 
            Regex regex = new Regex(@"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                    + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                    + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                                    + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$");
            if(!regex.IsMatch(value))
                throw new ArgumentException("Invalid IP address specified");
            _ipAddress = value;
        }
    }
    public string NetworkName
    {
        get => _networkName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Network name cannot be empty.");
            _networkName = value;
        }
    }

    public EmbeddedDevices(int id, string name, bool isOn, string ipAddress, string connectionName) : base(id, name, isOn)
    {
        IpName = ipAddress;
        NetworkName = connectionName;
    }

    public void Connect()
    {
        if(!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException("Cannot connect to a device. Must have MD Ltd.");
        Console.WriteLine($"Connecting to device: {NetworkName}");
    }

    protected override bool CanTurnOn()
    {
        return NetworkName.Contains("MD Ltd.");
    }

    public override void TurnOn()
    {
        Connect();
        if (!CanTurnOn())
            throw new ConnectionException("Device cannot be turned on due to network restrictions.");
        
        base.TurnOn();
    }

    public override string ToString()
    {
        return base.ToString() + $" | IP: {_ipAddress} - Network: {NetworkName}";
    }
}
public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message) { }
}

