using DeviceManager.Entities;

namespace DeviceManager.Logic;

public interface IDeviceRepository
{
    IEnumerable<Smartwatches> GetAllSmartwatches();
    bool AddSmartwatch(Smartwatches device);
    bool UpdateSmartwatch(string id, Smartwatches device);
    bool DeleteSmartwatch(string id);

    // Personal Computer
    IEnumerable<PersonalComputer> GetAllPersonalComputers();
    bool AddPersonalComputer(PersonalComputer device);
    bool UpdatePersonalComputer(string id, PersonalComputer device);
    bool DeletePersonalComputer(string id);

    // Embedded Device
    IEnumerable<EmbeddedDevices> GetAllEmbeddedDevices();
    bool AddEmbeddedDevice(EmbeddedDevices device);
    bool UpdateEmbeddedDevice(string id, EmbeddedDevices device);
    bool DeleteEmbeddedDevice(string id);
}