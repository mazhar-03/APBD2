using System.Text.Json.Serialization;

namespace DeviceManager.Entities;

/// <summary>
///     A smartwatch device that has a battery and can notify when low.
/// </summary>
public class Smartwatches : Device, IPowerNotifier
{
    private int _batteryPercentage;
    private bool _notifiedLowBattery;

    /// <summary>
    ///     Sets up the smartwatch with its info and battery level.
    /// </summary>
    /// <param name="id">The ID of the device.</param>
    /// <param name="name">The name of the smartwatch.</param>
    /// <param name="isOn">Whether it's on at the start.</param>
    /// <param name="batteryPercentage">Battery level (0–100%).</param>
    [JsonConstructor]
    public Smartwatches(string id, string name, bool isOn, int batteryPercentage) : base(id, name, isOn)
    {
        BatteryPercentage = batteryPercentage;
    }

    /// <summary>
    ///     The current battery level. Sends low battery alert if below 20%.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is not between 0 and 100.</exception>
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

    /// <summary>
    ///     Prints a notification message to the console.
    /// </summary>
    /// <param name="msg">The message to show.</param>
    public void Notify(string msg)
    {
        Console.WriteLine(msg);
    }

    /// <summary>
    ///     Turns the watch on. Needs at least 11% battery to work.
    /// </summary>
    /// <exception cref="EmptyBatteryException">Thrown if battery is too low.</exception>
    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
            throw new EmptyBatteryException("Smartwatches cannot turn on with battery less than 11%");
        BatteryPercentage -= 10;
        base.TurnOn();
    }

    /// <summary>
    ///     Returns device details in file-friendly format.
    /// </summary>
    public override string ToString()
    {
        return $"SW-{Id},{Name},{IsOn},{BatteryPercentage}%";
    }
}