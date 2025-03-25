using Project;

IDeviceRepository deviceRepository = new DeviceTextFileService("devices.txt");
var factory = new DeviceManagerFactory(deviceRepository);
var deviceManager = factory.CreateDeviceManager();
deviceManager.ShowAllDevices();