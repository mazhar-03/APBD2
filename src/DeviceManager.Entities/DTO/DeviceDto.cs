namespace DeviceManager.Entities.DTO;

public class DeviceDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public byte[] RowVersion { get; set; }
}