using Project;

var manager = DeviceManagerFactory.Create("devices.txt");
manager.AddDevice(new PersonalComputer("3", "Dell", true, "Windows 8"));

manager.ShowAllDevices();