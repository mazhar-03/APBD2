INSERT INTO Smartwatches (Id, Name, IsOn, BatteryPercentage) VALUES
    ('SW-01', 'Apple Watch', 1, 75),
    ('SW-02', 'Xiaomi Smartwatch', 0, 38),
    ('SW-03', 'Garmin Forerunner 55', 1, 98);

GO

INSERT INTO PersonalComputers (Id, Name, IsOn, OperatingSystem) VALUES
    ('P-01', 'Lenovo Legion 5', 0, 'Windows 10'),
    ('P-02', 'Dell QAZ', 1, 'Linux'),
    ('P-03', 'MacBook Air', 1, 'MacOS');

GO
INSERT INTO EmbeddedDevices (Id, Name, IsOn, IpName, NetworkName) VALUES
    ('ED-01', 'Sensor Unit', 1, '192.168.1.10', 'MD Ltd. Home Network'),
    ('ED-02', 'Extended Microphone Sensor', 1, '192.251.1.230', 'MD Ltd. Office Network'),
    ('ED-03', 'Audio Device', 1, '62.62.0.0', 'Open Network');
