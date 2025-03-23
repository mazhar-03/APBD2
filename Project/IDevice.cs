namespace Project;

public interface IDevice
{
    string Id { get; set; }
    string Name { get; set; }
    bool IsOn { get; set; }

    void TurnOn();
    void TurnOff();
    string ToString();
}