using Project;

IDeviceRepository deviceRepository = new DeviceTextFileService("devices.txt");
DeviceManagerFactory factory = new DeviceManagerFactory(deviceRepository);
IDeviceManager manager = factory.CreateDeviceManager();

manager.AddDevice(new Smartwatches("4", "Smartwatches", true, 73));
manager.ShowAllDevices();