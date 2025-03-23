using Project;

namespace DeviceManagerTests;

public class DeviceManagerTests
{
    [Fact]
    public void CheckEmptyBatteryException()
    {
        IDevice watch = new Smartwatches("2", "Sw22", false, 10);
        Assert.Throws<EmptyBatteryException>(() => watch.TurnOn());
    }

    [Fact]
    public void CheckAddDevice()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input1.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWhenStorageFull()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input2.txt");
        for (var i = 1; i <= 15; i++)
            deviceManager.AddDevice(new Smartwatches(i.ToString(), "Sw22", false, i + 40));

        IDevice extraDevice = new Smartwatches("16", "Extra Watch", false, 50);
        deviceManager.AddDevice(extraDevice);

        Assert.Equal(15, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWithExistingIdSameType()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input3.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new Smartwatches("2", "ABC", false, 73));

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckAddDeviceWithExistingIdDiffType()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input4.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new PersonalComputer("2", "ABC", false, "OS"));

        Assert.Equal(2, deviceManager.DeviceCount());
    }

    [Fact]
    public void CheckRemoveDevice()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input5.txt");
        deviceManager.AddDevice(new Smartwatches("2", "Sw22", false, 73));
        deviceManager.AddDevice(new Smartwatches("3", "Sw22", false, 73));
        deviceManager.RemoveDevice("2", "Smartwatches");

        Assert.Equal(1, deviceManager.DeviceCount());
    }

    [Fact]
    public void UpdateBatteryForSmartwatches()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input6.txt");
        IDevice watch = new Smartwatches("4", "Sw22", false, 70);
        deviceManager.AddDevice(watch);
        deviceManager.UpdateBattery("4", 42);

        Assert.Equal(42, ((Smartwatches)watch).BatteryPercentage);
    }

    [Fact]
    public void UpdateOperatingSystemForComputers()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input7.txt");
        IDevice currentComputer = new PersonalComputer("2", "PC", false, "Windows");
        deviceManager.AddDevice(currentComputer);
        deviceManager.UpdateOperatingSystem("2", "Mac OS");

        Assert.Equal("Mac OS", ((PersonalComputer)currentComputer).OperatingSystem);
    }

    [Fact]
    public void UpdateIpAddressForEmbeddedDevices()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input8.txt");
        IDevice device = new EmbeddedDevices("2", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        deviceManager.UpdateIpAddress("2", "111.111.111.111");

        Assert.Equal("111.111.111.111", ((EmbeddedDevices)device).IpName);
    }

    [Fact]
    public void UpdateNonValidIpAddressForEmbeddedDevices()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input9.txt");
        IDevice device = new EmbeddedDevices("1", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);

        Assert.Throws<ArgumentException>(() => deviceManager.UpdateIpAddress("1", "11.766.672.611"));
    }

    [Fact]
    public void UpdateNetworkNameForEmbeddedDevices()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input10.txt");
        IDevice device = new EmbeddedDevices("4", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        deviceManager.UpdateNetworkName("4", "Network Name MD Ltd. consist");

        Assert.Equal("Network Name MD Ltd. consist", ((EmbeddedDevices)device).NetworkName);
    }

    [Fact]
    public void UpdateNonValidNetworkNameForEmbeddedDevices()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input11.txt");
        IDevice device = new EmbeddedDevices("1", "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);

        Assert.Throws<ConnectionException>(() => deviceManager.UpdateNetworkName("1", "c# party"));
    }

    [Fact]
    public void UpdateTurnOn()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input12.txt");
        IDevice watch = new Smartwatches("3", "Sw22", false, 100);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOnDevice("3", "Smartwatches");

        Assert.True(watch.IsOn);
    }

    [Fact]
    public void UpdateTurnOff()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input13.txt");
        IDevice watch = new Smartwatches("3", "Sw22", true, 80);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOffDevice("3", "Smartwatches");

        Assert.False(watch.IsOn);
    }

    [Fact]
    public void TurnOnPcWithoutAnOs()
    {
        IDeviceManager deviceManager = DeviceManagerFactory.Create("input14.txt");
        IDevice currentComputer = new PersonalComputer("5", "PC", false, "");
        deviceManager.AddDevice(currentComputer);
        Assert.Throws<EmptySystemException>(() => deviceManager.TurnOnDevice("5", "PersonalComputer"));
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly()
    {
        var testFilePath = "input15.txt";
        IDeviceManager deviceManager = DeviceManagerFactory.Create(testFilePath);
        IDevice device = new Smartwatches("1", "Apple Watch", false, 50);

        deviceManager.AddDevice(device);

        var lines = File.ReadAllLines(testFilePath);
        Assert.Single(lines);
        Assert.Equal("SW-1,Apple Watch,False,50%", lines[0]);
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly2()
    {
        var testFilePath = "input16.txt";
        IDeviceManager deviceManager = DeviceManagerFactory.Create(testFilePath);
        IDevice device = new Smartwatches("3", "Apple Watch", false, 40);

        deviceManager.AddDevice(device);
        deviceManager.TurnOnDevice("3", "Smartwatches");

        var lines = File.ReadAllLines(testFilePath);
        Assert.Single(lines);
        Assert.Equal("SW-3,Apple Watch,True,30%", lines[0]);
    }
}