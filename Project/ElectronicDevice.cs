namespace Project;

public abstract class ElectronicDevice
{
    public ElectronicDevice(string id, string name, bool isOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty");
        Id = id;
        Name = name ?? throw new ArgumentException("Name cannot be null");
        IsOn = isOn;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }

    public virtual void TurnOn()
    {
        if (IsOn)
            throw new InvalidOperationException("Device is already turned on.");
        IsOn = true;
    }

    public virtual void TurnOff()
    {
        if (!IsOn)
            throw new InvalidOperationException("Device is already turned off.");
        IsOn = false;
    }

    public override string ToString()
    {
        return $"Device [ID: {Id}, Name: {Name}, Status: {(IsOn ? "On" : "Off")}]";
    }
}