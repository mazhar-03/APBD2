using DeviceManager.Entities;
using DeviceManager.Entities.DTO;
using DeviceManager.Repositories;

namespace DeviceManager.Logic;

public class DeviceService : IDeviceService
{
    private readonly IDeviceDBRepository _repository;

        public DeviceService(IDeviceDBRepository repo)
        {
            _repository = repo;
        }

        public IEnumerable<DeviceDto> GetAllDevices() => _repository.GetAllDevices();

        public DeviceDto? GetDeviceById(string id)
        {
            if (!_repository.DeviceExists(id))
                return null;

            return id switch
            {
                var d when d.StartsWith("p-", StringComparison.OrdinalIgnoreCase) => _repository.GetPersonalComputerById(id),
                var d when d.StartsWith("sw-", StringComparison.OrdinalIgnoreCase) => _repository.GetSmartwatchById(id),
                var d when d.StartsWith("ed-", StringComparison.OrdinalIgnoreCase) => _repository.GetEmbeddedDevicesById(id),
                _ => null
            };
        }

        public bool AddSmartwatch(Smartwatches device) => _repository.AddSmartwatch(device);
        public bool AddPersonalComputer(PersonalComputer device) => _repository.AddPersonalComputer(device);
        public bool AddEmbedded(EmbeddedDevices device) => _repository.AddEmbedded(device);
        public bool UpdateDevice(DeviceDto deviceDto) => _repository.UpdateDevice(deviceDto);
        public bool DeleteDevice(string id)
        {
            if (!_repository.DeviceExists(id))
                return false;

            return id switch
            {
                var d when d.StartsWith("p-", StringComparison.OrdinalIgnoreCase) => _repository.DeletePersonalComputer(id),
                var d when d.StartsWith("sw-", StringComparison.OrdinalIgnoreCase) => _repository.DeleteSmartwatch(id),
                var d when d.StartsWith("ed-", StringComparison.OrdinalIgnoreCase) => _repository.DeleteEmbeddedDevice(id),
                _ => false
            };
        }

        public string GenerateNewId(string deviceType) => _repository.GenerateNewId(deviceType);
        public bool DeviceExists(string id) => _repository.DeviceExists(id);
    }
