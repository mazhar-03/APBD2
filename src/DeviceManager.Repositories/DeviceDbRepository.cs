using DeviceManager.Entities;
using DeviceManager.Entities.DTO;
using Microsoft.Data.SqlClient;

namespace DeviceManager.Repositories;

public class DeviceDbRepository : IDeviceDBRepository
{
    private readonly string _connectionString;

    public DeviceDbRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GenerateNewId(string deviceType)
    {
        var strType = deviceType.ToUpper() switch
        {
            "SW" => "SW-",
            "P" => "P-",
            "ED" => "ED-",
            _ => throw new ArgumentException("Not a valid device type.")
        };

        var maxNumber = 0;

        var sql = "SELECT Id FROM Device WHERE Id LIKE @Prefix";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Prefix", strType + "%");
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var idString = reader.GetString(0);
                    var parts = idString.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var num))
                        if (num > maxNumber)
                            maxNumber = num;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        return $"{strType}{maxNumber + 1:D2}";
    }


    public IEnumerable<DeviceDto> GetAllDevices()
    {
        var devices = new List<DeviceDto>();
        var sql = "SELECT * FROM Device";

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand(sql, conn);

            var reader = cmd.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var device = new DeviceDto
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2)
                        );
                        devices.Add(device);
                    }
            }
            finally
            {
                reader.Close();
            }
        }

        return devices;
    }

    public Smartwatches GetSmartwatchById(string id)
    {
        Smartwatches smartwatch = null;
        var sql =
            "SELECT d.Id, d.Name, d.IsEnabled, s.BatteryPercentage FROM Device d JOIN Smartwatch s ON d.Id = s.DeviceId WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                        smartwatch = new Smartwatches
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2),
                            reader.GetInt32(3)
                        );
            }
            finally
            {
                reader.Close();
            }
        }

        return smartwatch;
    }

    public bool AddSmartwatch(Smartwatches device)
    {
        var insertDevice = "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)";
        var insertSw =
            "INSERT INTO Smartwatch (Id, BatteryPercentage, DeviceId) VALUES (@SwId, @BatteryPercentage, @DeviceId)";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();

            try
            {
                var deviceCommand = new SqlCommand(insertDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", device.Id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var swCommand = new SqlCommand(insertSw, connection, transaction);
                var swId = GetIntId(device);
                swCommand.Parameters.AddWithValue("@SwId", swId);
                swCommand.Parameters.AddWithValue("@BatteryPercentage", device.BatteryPercentage);
                swCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                swCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool UpdateSmartwatch(string id, Smartwatches device)
    {
        var updateDevice = "UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id";
        var updateSw = "UPDATE Smartwatch SET BatteryPercentage = @BatteryPercentage WHERE DeviceId = @DeviceId";
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();

            try
            {
                var deviceCommand = new SqlCommand(updateDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var swCommand = new SqlCommand(updateSw, connection, transaction);
                var swId = GetIntId(device);
                swCommand.Parameters.AddWithValue("@SwId", swId);
                swCommand.Parameters.AddWithValue("@BatteryPercentage", device.BatteryPercentage);
                swCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                swCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool DeleteSmartwatch(string id)
    {
        var deleteSw = "DELETE FROM Smartwatch WHERE DeviceId = @Id";
        var deleteDevice = "DELETE FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var swCommand = new SqlCommand(deleteSw, connection, transaction);
                swCommand.Parameters.AddWithValue("@Id", id);
                swCommand.ExecuteNonQuery();

                var deviceCommand = new SqlCommand(deleteDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public PersonalComputer GetPersonalComputerById(string id)
    {
        PersonalComputer pc = null;
        var sql =
            "SELECT d.Id, d.Name, d.IsEnabled, pc.OperationSystem FROM Device d JOIN PersonalComputer pc ON d.Id = pc.DeviceId WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.Read())
                    pc = new PersonalComputer(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetBoolean(2),
                        reader.IsDBNull(3) ? null : reader.GetString(3)
                    );
            }
            finally
            {
                reader.Close();
            }
        }

        return pc;
    }

    public bool AddPersonalComputer(PersonalComputer device)
    {
        var insertDevice = "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)";
        var insertPC =
            "INSERT INTO PersonalComputer (Id, OperationSystem, DeviceId) VALUES (@PcId, @OperationSystem, @DeviceId)";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var deviceCommand = new SqlCommand(insertDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", device.Id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var pcCommand = new SqlCommand(insertPC, connection, transaction);
                var pcId = GetIntId(device);
                pcCommand.Parameters.AddWithValue("@PcId", pcId);
                pcCommand.Parameters.AddWithValue("@OperationSystem", device.OperatingSystem ?? (object)DBNull.Value);
                pcCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                pcCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool UpdatePersonalComputer(string id, PersonalComputer device)
    {
        var updateDevice = "UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id";
        var updatePC = "UPDATE PersonalComputer SET OperationSystem = @OperationSystem WHERE DeviceId = @DeviceId";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var deviceCommand = new SqlCommand(updateDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var pcCommand = new SqlCommand(updatePC, connection, transaction);
                pcCommand.Parameters.AddWithValue("@OperationSystem", device.OperatingSystem ?? (object)DBNull.Value);
                pcCommand.Parameters.AddWithValue("@DeviceId", id);
                pcCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool DeletePersonalComputer(string id)
    {
        var deletePC = "DELETE FROM PersonalComputer WHERE DeviceId = @Id";
        var deleteDevice = "DELETE FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var pcCommand = new SqlCommand(deletePC, connection, transaction);
                pcCommand.Parameters.AddWithValue("@Id", id);
                pcCommand.ExecuteNonQuery();

                var deviceCommand = new SqlCommand(deleteDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public EmbeddedDevices GetEmbeddedDevicesById(string id)
    {
        EmbeddedDevices ed = null;
        var sql =
            "SELECT d.Id, d.Name, d.IsEnabled, e.IpAddress, e.NetworkName FROM Device d JOIN Embedded e ON d.Id = e.DeviceId WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.Read())
                    ed = new EmbeddedDevices(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetBoolean(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    );
            }
            finally
            {
                reader.Close();
            }
        }

        return ed;
    }

    public bool AddEmbeddedDevice(EmbeddedDevices device)
    {
        var insertDevice = "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)";
        var insertED =
            "INSERT INTO Embedded (Id, IpAddress, NetworkName, DeviceId) VALUES (@EdId, @Ip, @Network, @DeviceId)";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var deviceCommand = new SqlCommand(insertDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", device.Id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var edId = GetIntId(device);

                var edCommand = new SqlCommand(insertED, connection, transaction);
                edCommand.Parameters.AddWithValue("@EdId", edId);
                edCommand.Parameters.AddWithValue("@Ip", device.IpName);
                edCommand.Parameters.AddWithValue("@Network", device.NetworkName);
                edCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                edCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool UpdateEmbeddedDevice(string id, EmbeddedDevices device)
    {
        var updateDevice = "UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id";
        var updateED = "UPDATE Embedded SET IpAddress = @Ip, NetworkName = @Network WHERE DeviceId = @DeviceId";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var deviceCommand = new SqlCommand(updateDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.Parameters.AddWithValue("@Name", device.Name);
                deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                deviceCommand.ExecuteNonQuery();

                var edCommand = new SqlCommand(updateED, connection, transaction);
                edCommand.Parameters.AddWithValue("@Ip", device.IpName);
                edCommand.Parameters.AddWithValue("@Network", device.NetworkName);
                edCommand.Parameters.AddWithValue("@DeviceId", id);
                edCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool DeleteEmbeddedDevice(string id)
    {
        var deleteED = "DELETE FROM Embedded WHERE DeviceId = @Id";
        var deleteDevice = "DELETE FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var edCommand = new SqlCommand(deleteED, connection, transaction);
                edCommand.Parameters.AddWithValue("@Id", id);
                edCommand.ExecuteNonQuery();

                var deviceCommand = new SqlCommand(deleteDevice, connection, transaction);
                deviceCommand.Parameters.AddWithValue("@Id", id);
                deviceCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }

    public bool DeviceExists(string id)
    {
        var sql = "SELECT COUNT(*) FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }

    private static int GetIntId(Device device)
    {
        return int.Parse(device.Id.Split('-')[1]);
    }
}