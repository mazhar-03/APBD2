using Project;

IDeviceManager manager = DeviceManagerFactory.Create("devices.txt");
IDevice device = new EmbeddedDevices("1", "Embedeed", false, "123.123.123.123", "MD Ltd. home");

manager.EditDevice("1", "EmbeddedDevices", "Embedded");
manager.ShowAllDevices();