using DeviceManager.Entities;
using DeviceManager.Entities.DTO;

namespace DeviceManager.Repositories;

public interface IDeviceDBRepository
{
    //General
    IEnumerable<DeviceDto> GetAllDevices();
    bool DeviceExists(string id);

    // Smartwatches
    SmartwatchDto? GetSmartwatchById(string id);
    bool AddSmartwatch(Smartwatches device);
    bool UpdateSmartwatch(string id, SmartwatchDto device);
    bool DeleteSmartwatch(string id);

    // Personal Computer
    PersonalComputerDto? GetPersonalComputerById(string id);
    bool AddPersonalComputer(PersonalComputer device);
    bool UpdatePersonalComputer(string id, PersonalComputerDto device);
    bool DeletePersonalComputer(string id);

    // Embedded Device
    EmbeddedDto? GetEmbeddedDevicesById(string id);
    bool AddEmbedded(EmbeddedDevices device);
    bool UpdateEmbeddedDevice(string id, EmbeddedDto device);
    bool DeleteEmbeddedDevice(string id);
    string GenerateNewId(string deviceType);
}