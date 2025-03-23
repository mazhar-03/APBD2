using Project;

var manager = DeviceManagerFactory.Create("devices.txt");
manager.AddDevice(new PersonalComputer("2", "Lenovo", false, "Windows 11"));

manager.ShowAllDevices();