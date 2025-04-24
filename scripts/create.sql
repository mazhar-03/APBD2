CREATE TABLE Device (
                        Id NVARCHAR(10) PRIMARY KEY,
                        Name NVARCHAR(30) NOT NULL,
                        IsEnabled BIT NOT NULL
);
GO

CREATE TABLE Smartwatch (
                            Id INT PRIMARY KEY,
                            BatteryPercentage INT NOT NULL,
                            DeviceId NVARCHAR(10) NOT NULL,
                            FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO

CREATE TABLE PersonalComputer (
                                  Id INT PRIMARY KEY,
                                  OperationSystem NVARCHAR(20) NULL,
                                  DeviceId NVARCHAR(10) NOT NULL,
                                  FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO

CREATE TABLE Embedded (
                          Id INT PRIMARY KEY,
                          IpAddress NVARCHAR(20) NOT NULL,
                          NetworkName NVARCHAR(30) NOT NULL,
                          DeviceId NVARCHAR(10) NOT NULL,
                          FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO