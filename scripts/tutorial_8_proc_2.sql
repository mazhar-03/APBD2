ALTER PROCEDURE AddSmartwatch
    @SwId               INT,
    @DeviceId           VARCHAR(50),
    @Name               NVARCHAR(100),
    @IsEnabled          BIT,
    @BatteryPercentage  INT
    AS
BEGIN
    SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;

INSERT INTO Device       (Id, Name,  IsEnabled)
VALUES                   (@DeviceId, @Name, @IsEnabled);

INSERT INTO Smartwatch   (Id, BatteryPercentage, DeviceId)
VALUES                   (@SwId, @BatteryPercentage, @DeviceId);

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
        THROW;
END CATCH
END
