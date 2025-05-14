
using DeviceManager.Entities;
using DeviceManager.Entities.DTO;

namespace DeviceManager.Logic;

public interface IDeviceService
{
    IEnumerable<DeviceDto> GetAllDevices();
    DeviceDto? GetDeviceById(string id);
    bool AddSmartwatch(Smartwatches device);
    bool AddPersonalComputer(PersonalComputer device);
    bool AddEmbedded(EmbeddedDevices device);
    bool UpdateDevice(DeviceDto deviceDto);
    bool DeleteDevice(string id);
    string GenerateNewId(string deviceType);
    bool DeviceExists(string id);
}