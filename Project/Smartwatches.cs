namespace Project;

public class Smartwatches : ElectronicDevice, IPowerNotifier
{
    private int _batteryPercentage;
    private bool _notifiedLowBattery;

    public Smartwatches(string id, string name, bool isOn, int batteryPercentage) : base(id, name, isOn)
    {
        BatteryPercentage = batteryPercentage;
    }

    public int BatteryPercentage
    {
        get => _batteryPercentage;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentException("Battery percentage must be between 0 and 100");

            _batteryPercentage = value;
            if (_batteryPercentage < 20 && !_notifiedLowBattery)
            {
                Notify("Smartwatch's battery is low!");
                _notifiedLowBattery = true;
            }
        }
    }

    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
            throw new EmptyBatteryException("Smartwatches cannot turn on with battery less than 11%");
        BatteryPercentage -= 10;
        base.TurnOn();
    }

    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"SW-{Id},{Name},{IsOn},{BatteryPercentage}%";
    }
}