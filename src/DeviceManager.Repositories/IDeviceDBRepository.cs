using DeviceManager.Entities;
using DeviceManager.Entities.DTO;

namespace DeviceManager.Repositories;

public interface IDeviceDBRepository
{
    //General
    IEnumerable<DeviceDto> GetAllDevices();
    bool DeviceExists(string id);
    string GenerateNewId(string deviceType);
    bool UpdateDevice(DeviceDto deviceDto);
    DeviceDto? GetDeviceById(string id);

    // Smartwatches
    bool AddSmartwatch(Smartwatches device);
    bool DeleteSmartwatch(string id);
    SmartwatchDto? GetSmartwatchById(string id);

    // Personal Computer
    bool AddPersonalComputer(PersonalComputer device);
    bool DeletePersonalComputer(string id);
    PersonalComputerDto? GetPersonalComputerById(string id);

    // Embedded Device
    bool AddEmbedded(EmbeddedDevices device);
    bool DeleteEmbeddedDevice(string id);
    EmbeddedDto? GetEmbeddedDevicesById(string id);
}