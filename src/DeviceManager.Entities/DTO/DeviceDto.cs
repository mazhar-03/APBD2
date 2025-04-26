namespace DeviceManager.Entities.DTO;

public class DeviceDto
{
    public DeviceDto(string id, string name, bool isOn)
    {
        Id = id;
        Name = name;
        IsOn = isOn;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsOn { get; set; }
}