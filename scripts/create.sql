CREATE TABLE Smartwatches (
                              Id NVARCHAR(5) PRIMARY KEY,
                              Name NVARCHAR(30),
                              IsOn BIT,
                              BatteryPercentage INT
);
GO

CREATE TABLE PersonalComputers (
                                   Id NVARCHAR(5) PRIMARY KEY,
                                   Name NVARCHAR(30),
                                   IsOn BIT,
                                   OperatingSystem NVARCHAR(30)
);
GO

CREATE TABLE EmbeddedDevices (
                                 Id NVARCHAR(5) PRIMARY KEY,
                                 Name NVARCHAR(30),
                                 IsOn BIT,
                                 IpName NVARCHAR(20),
                                 NetworkName NVARCHAR(40)
);
