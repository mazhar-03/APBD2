using DeviceManager.Entities;
using DeviceManager.Entities.DTO;

namespace DeviceManager.Repositories;

public interface IDeviceDBRepository
{
    //General
    IEnumerable<DeviceDto> GetAllDevices();
    bool DeviceExists(string id);

    // Smartwatches
    Smartwatches GetSmartwatchById(string id);
    bool AddSmartwatch(Smartwatches device);
    bool UpdateSmartwatch(string id, Smartwatches device);
    bool DeleteSmartwatch(string id);

    // Personal Computer
    PersonalComputer GetPersonalComputerById(string id);
    bool AddPersonalComputer(PersonalComputer device);
    bool UpdatePersonalComputer(string id, PersonalComputer device);
    bool DeletePersonalComputer(string id);

    // Embedded Device
    EmbeddedDevices GetEmbeddedDevicesById(string id);
    bool AddEmbedded(EmbeddedDevices device);
    bool UpdateEmbeddedDevice(string id, EmbeddedDevices device);
    bool DeleteEmbeddedDevice(string id);
    string GenerateNewId(string deviceType);
}