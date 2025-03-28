using Project;

namespace DeviceManagerTests;

public class DeviceManagerTests
{
    [Fact]
    public void CheckEmptyBatteryException()
    {
        Device watch = new Smartwatches("2", "Sw22", false, 10);
        Assert.Throws<EmptyBatteryException>(() => watch.TurnOn());
    }

    [Fact]
    public void CheckAddDevice()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input1.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWhenStorageFull()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input2.txt");
        for (var i = 1; i <= 15; i++)
            deviceManager.AddDevice(new Smartwatches(i.ToString(), "Sw22", false, i + 40));

        Device extraDevice = new Smartwatches("16", "Extra Watch", false, 50);
        deviceManager.AddDevice(extraDevice);

        Assert.Equal(15, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWithExistingIdSameType()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input3.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new Smartwatches("2", "ABC", false, 73));

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWithExistingIdDiffType()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input4.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new PersonalComputer("2", "ABC", false, "OS"));

        Assert.Equal(2, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckRemoveDevice()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input5.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new Smartwatches("3", "Sw22", false, 73));
        deviceManager.RemoveDevice("2", "Smartwatches");

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void UpdateBatteryForSmartwatches()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input6.txt");
        Device watch = new Smartwatches("4", "Sw22", false, 70);
        deviceManager.AddDevice(watch);
        deviceManager.UpdateBattery("4", 42);

        Assert.Equal(42, ((Smartwatches)watch).BatteryPercentage);
    }

    [Fact]
    public void UpdateOperatingSystemForComputers()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input7.txt");
        Device currentComputer = new PersonalComputer("2", "PC", false, "Windows");
        deviceManager.AddDevice(currentComputer);
        deviceManager.UpdateOperatingSystem("2", "Mac OS");

        Assert.Equal("Mac OS", ((PersonalComputer)currentComputer).OperatingSystem);
    }

    [Fact]
    public void UpdateIpAddressForEmbeddedDevices()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input8.txt");
        Device device = new EmbeddedDevices("2", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        deviceManager.UpdateIpAddress("2", "111.111.111.111");

        Assert.Equal("111.111.111.111", ((EmbeddedDevices)device).IpName);
    }

    [Fact]
    public void UpdateNonValidIpAddressForEmbeddedDevices()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input9.txt");
        Device device = new EmbeddedDevices("1", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);

        Assert.Throws<ArgumentException>(() => deviceManager.UpdateIpAddress("1", "11.766.672.611"));
    }

    [Fact]
    public void UpdateNetworkNameForEmbeddedDevices()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input10.txt");
        Device device = new EmbeddedDevices("4", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        deviceManager.UpdateNetworkName("4", "Network Name MD Ltd. consist");

        Assert.Equal("Network Name MD Ltd. consist", ((EmbeddedDevices)device).NetworkName);
    }

    [Fact]
    public void UpdateNonValidNetworkNameForEmbeddedDevices()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input11.txt");
        Device device = new EmbeddedDevices("1", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);

        Assert.Throws<ConnectionException>(() => deviceManager.UpdateNetworkName("1", "c# party"));
    }

    [Fact]
    public void UpdateTurnOn()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input12.txt");
        Device watch = new Smartwatches("3", "Sw22", false, 100);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOnDevice("3", "Smartwatches");

        Assert.True(watch.IsOn);
    }

    [Fact]
    public void UpdateTurnOff()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input13.txt");
        Device watch = new Smartwatches("3", "Sw22", true, 80);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOffDevice("3", "Smartwatches");

        Assert.False(watch.IsOn);
    }

    [Fact]
    public void TurnOnPcWithoutAnOs()
    {
        var deviceManager = DeviceManagerFactory.CreateDeviceManager("input14.txt");
        Device currentComputer = new PersonalComputer("5", "PC", false, "");
        deviceManager.AddDevice(currentComputer);
        Assert.Throws<EmptySystemException>(() => deviceManager.TurnOnDevice("5", "PersonalComputer"));
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly()
    {
        var testFilePath = "input15.txt";
        var deviceManager = DeviceManagerFactory.CreateDeviceManager(testFilePath);
        Device device = new Smartwatches("1", "Apple Watch", false, 50);

        deviceManager.AddDevice(device);

        var lines = File.ReadAllLines(testFilePath);
        Assert.Single(lines);
        Assert.Equal("SW-1,Apple Watch,False,50%", lines[0]);
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly2()
    {
        var testFilePath = "input16.txt";
        var deviceManager = DeviceManagerFactory.CreateDeviceManager(testFilePath);
        Device device = new Smartwatches("3", "Apple Watch", false, 40);

        deviceManager.AddDevice(device);
        deviceManager.TurnOnDevice("3", "Smartwatches");

        var lines = File.ReadAllLines(testFilePath);
        Assert.Single(lines);
        Assert.Equal("SW-3,Apple Watch,True,30%", lines[0]);
    }
}