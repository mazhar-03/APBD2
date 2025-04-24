INSERT INTO Device (Id, Name, IsEnabled) VALUES
                                             ('SW-01', 'Apple Watch', 1),
                                             ('SW-02', 'Xiaomi Smartwatch', 0),
                                             ('SW-03', 'Garmin Forerunner 55', 1),
                                             ('P-01', 'Lenovo Legion 5', 0),
                                             ('P-02', 'Dell QAZ', 1),
                                             ('P-03', 'MacBook Air', 1),
                                             ('ED-01', 'Sensor Unit', 1),
                                             ('ED-02', 'Extended Microphone Sensor', 1),
                                             ('ED-03', 'Audio Device', 1);
GO

INSERT INTO Smartwatch (Id, BatteryPercentage, DeviceId) VALUES
                                                             (01, 75, 'SW-01'),
                                                             (02, 38, 'SW-02'),
                                                             (03, 98, 'SW-03');
GO

INSERT INTO PersonalComputer (Id, OperationSystem, DeviceId) VALUES
                                                                 (01, 'Windows 10', 'P-01'),
                                                                 (02, 'Linux', 'P-02'),
                                                                 (03, 'MacOS', 'P-03');
GO

INSERT INTO Embedded (Id, IpAddress, NetworkName, DeviceId) VALUES
                                                                (01, '192.168.1.10', 'MD Ltd. Home Network', 'ED-01'),
                                                                (02, '192.251.1.230', 'MD Ltd. Office Network', 'ED-02'),
                                                                (03, '62.62.0.0', 'Open Network', 'ED-03');
GO
