namespace DeviceManagerTests;

public class DeviceManagerTests

{
    [Fact]
    public void CheckEmptyBatteryException()
    {
        var watch = new Smartwatches(2, "Sw22", false, 10);
        Assert.Throws<EmptyBatteryException>(() => watch.TurnOn());
    }

    [Fact]
    public void CheckAddDevice()
    {
        var deviceManager = new DeviceManager("input1.txt");
        deviceManager.AddDevice(new Smartwatches(2, "Sw22", false, 73));

        var expectedResult = 1;
        var actualResult = deviceManager.DeviceCount;
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void CheckAddDeviceWhenStorageFull()
    {
        var deviceManager = new DeviceManager("input2.txt");
        for (var i = 1; i <= 15; i++)
            deviceManager.AddDevice(new Smartwatches(i, "Sw22", false, i + 40));

        var extraDevice = new Smartwatches(16, "Extra Watch", false, 50);
        deviceManager.AddDevice(extraDevice);
        Assert.Equal(15, deviceManager.DeviceCount);
    }

    [Fact]
    public void CheckAddDeviceWithExistingId()
    {
        var deviceManager = new DeviceManager("input3.txt");
        deviceManager.AddDevice(new Smartwatches(2, "Sw22", false, 73));
        deviceManager.AddDevice(new Smartwatches(2, "ABC", false, 73));

        Assert.Equal(1, deviceManager.DeviceCount);
    }

    [Fact]
    public void CheckRemoveDevice()
    {
        var deviceManager = new DeviceManager("input4.txt");
        deviceManager.AddDevice(new Smartwatches(2, "Sw22", false, 73));
        deviceManager.RemoveDevice(2);
        Assert.Equal(0, deviceManager.DeviceCount);
    }

    [Fact]
    public void UpdateBatteryForSmartwatches()
    {
        var deviceManager = new DeviceManager("input5.txt");
        var watch = new Smartwatches(4, "Sw22", false, 70);
        deviceManager.AddDevice(watch);
        deviceManager.UpdateBattery(4, 42);
        Assert.Equal(42, watch.BatteryPercentage);
    }

    [Fact]
    public void UpdateOperatingSystemForComputers()
    {
        var deviceManager = new DeviceManager("input6.txt");
        var currentComputer = new PersonalComputer(2, "PC", false, "Windows");
        deviceManager.AddDevice(currentComputer);
        var newOS = "Mac OS";
        deviceManager.UpdateOperatingSystem(2, newOS);
        Assert.Equal(newOS, currentComputer.OperatingSystem);
    }

    [Fact]
    public void UpdateIpAddressForEmbeddedDevices()
    {
        var deviceManager = new DeviceManager("input7.txt");
        var device =
            new EmbeddedDevices(1, "eMBEDDEEED Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        var newIp = "111.111.111.111";
        deviceManager.UpdateIpAddress(1, newIp);
        Assert.Equal(newIp, device.IpName);
    }

    [Fact]
    public void UpdateNonValidIpAddressForEmbeddedDevices()
    {
        var deviceManager = new DeviceManager("input7.txt");
        var device = new EmbeddedDevices(1, "Embedded Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        var invalidIp = "11.766.672.611";
        Assert.Throws<ArgumentException>(() => deviceManager.UpdateIpAddress(1, invalidIp));
    }

    [Fact]
    public void UpdateNetworkNameForEmbeddedDevices()
    {
        var deviceManager = new DeviceManager("input9.txt");
        var device =
            new EmbeddedDevices(1, "eMBEDDEEED Device", false, "123.123.123.123", "MD Ltd. consist");
        var newNetworkName = "Network Name MD Ltd. consist";
        deviceManager.AddDevice(device);
        deviceManager.UpdateNetworkName(1, newNetworkName);
        Assert.Equal(newNetworkName, device.NetworkName);
    }

    [Fact]
    public void UpdateNonValidNetworkNameForEmbeddedDevices()
    {
        var deviceManager = new DeviceManager("input8.txt");
        var device =
            new EmbeddedDevices(1, "eMBEDDEEED Device", false, "123.123.123.123", "MD Ltd. consist");
        deviceManager.AddDevice(device);
        var newConnectionName = "c# party";
        Assert.Throws<ConnectionException>(() => deviceManager.UpdateNetworkName(1, newConnectionName));
    }

    [Fact]
    public void UpdateTurnOn()
    {
        var deviceManager = new DeviceManager("input10.txt");
        var watch = new Smartwatches(2, "Sw22", false, 100);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOnDevice(2, "Smartwatches");
        Assert.True(watch.IsOn);
    }

    [Fact]
    public void UpdateTurnOff()
    {
        var deviceManager = new DeviceManager("input11.txt");
        var watch = new Smartwatches(2, "Sw22", true, 80);
        deviceManager.AddDevice(watch);
        deviceManager.TurnOffDevice(2, "Smartwatches");
        Assert.False(watch.IsOn);
    }

    [Fact]
    public void TurnOnPcWithoutAnOs()
    {
        var deviceManager = new DeviceManager("input12.txt");
        var currentComputer = new PersonalComputer(4, "PC", false, "");
        deviceManager.AddDevice(currentComputer);
        Assert.Throws<EmptySystemException>(() => deviceManager.TurnOnDevice(4, "PersonalComputer"));
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly()
    {
        var testFilePath = "input14.txt";
        var deviceManager = new DeviceManager(testFilePath);
        var device = new Smartwatches(1, "Apple Watch", false, 50);

        deviceManager.AddDevice(device);
        deviceManager.SaveToFile();

        var lines = File.ReadAllLines(testFilePath);
        Assert.Single(lines);
        Assert.Equal("SW-1,Apple Watch,False,50%", lines[0]);
    }

    [Fact]
    public void CheckSaveToFileWorksCorrectly2()
    {
        var testFilePath = "input17.txt";
        var deviceManager = new DeviceManager(testFilePath);
        var device = new Smartwatches(3, "Apple Watch", false, 40);

        deviceManager.AddDevice(device);
        deviceManager.TurnOnDevice(3, "Smartwatches");

        var lines = File.ReadAllLines(testFilePath);
        Assert.Equal("SW-3,Apple Watch,True,30%", lines[0]);
    }
}