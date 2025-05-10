CREATE PROCEDURE AddSmartwatch
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsEnabled BIT,
    @BatteryPercentage INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsEnabled)
        VALUES (@DeviceId, @Name, @IsEnabled);

        -- Insert into Smartwatch table
        INSERT INTO Smartwatch (BatteryPercentage, DeviceId)
        VALUES (@BatteryPercentage, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
