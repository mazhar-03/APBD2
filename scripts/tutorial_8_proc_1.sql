CREATE PROCEDURE AddEmbedded
    @EdId            INT,
  @DeviceId        VARCHAR(50),
  @Name            NVARCHAR(100),
  @IsEnabled       BIT,
  @IpAddress       VARCHAR(100),
  @NetworkName     VARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;

INSERT INTO Device (Id, Name, IsEnabled)
VALUES (@DeviceId, @Name, @IsEnabled);

INSERT INTO Embedded (Id, IpAddress, NetworkName, DeviceId)
VALUES (@EdId, @IpAddress, @NetworkName, @DeviceId);

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
    THROW;
END CATCH
END
GO