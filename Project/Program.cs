using Project;

IDeviceManager manager = DeviceManagerFactory.Create("devices.txt");
manager.ShowAllDevices();