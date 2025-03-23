namespace Project;

public class PersonalComputer : ElectronicDevice
{
    public PersonalComputer(string id, string name, bool isOn, string operatingSystem) : base(id, name, isOn)
    {
        OperatingSystem = operatingSystem;
        if (isOn && string.IsNullOrWhiteSpace(operatingSystem))
            throw new EmptySystemException("PC cannot be created as ON without an operating system.");
    }

    public string OperatingSystem { get; set; }

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
            throw new EmptySystemException("PC cannot be launched without an operating system.");
        base.TurnOn();
    }

    public override void TurnOff()
    {
        base.TurnOff();
    }

    public override string ToString()
    {
        return $"P-{Id},{Name},{IsOn},{OperatingSystem}";
    }
}