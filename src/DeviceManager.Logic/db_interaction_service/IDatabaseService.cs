using DeviceManager.Entities;

namespace DeviceManager.Logic;

public interface IDatabaseService
{
    IEnumerable<Smartwatches> GetAllSmartwatches();
    Smartwatches GetSmartwatchById(string id);
    bool AddSmartwatch(Smartwatches device);
    bool UpdateSmartwatch(string id, Smartwatches device);
    bool DeleteSmartwatch(string id);

    // Personal Computer
    IEnumerable<PersonalComputer> GetAllPersonalComputers();
    PersonalComputer GetPersonalComputerById(string id);
    bool AddPersonalComputer(PersonalComputer device);
    bool UpdatePersonalComputer(string id, PersonalComputer device);
    bool DeletePersonalComputer(string id);

    // Embedded Device
    IEnumerable<EmbeddedDevices> GetAllEmbeddedDevices();
    EmbeddedDevices GetEmbeddedDevicesById(string id);
    bool AddEmbeddedDevice(EmbeddedDevices device);
    bool UpdateEmbeddedDevice(string id, EmbeddedDevices device);
    bool DeleteEmbeddedDevice(string id);
    bool DeviceExists(string id);
}