CREATE PROCEDURE AddPersonalComputer
    @PcId             INT,
  @DeviceId         VARCHAR(50),
  @Name             NVARCHAR(100),
  @IsEnabled        BIT,
  @OperationSystem  NVARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;

INSERT INTO Device (Id, Name, IsEnabled)
VALUES (@DeviceId, @Name, @IsEnabled);

INSERT INTO PersonalComputer (Id, OperationSystem, DeviceId)
VALUES (@PcId, @OperationSystem, @DeviceId);

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
    THROW;
END CATCH
END
GO