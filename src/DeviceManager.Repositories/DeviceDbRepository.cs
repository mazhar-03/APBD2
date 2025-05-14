using System.Data;
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
        const string sql = "SELECT Id FROM Device WHERE Id LIKE @Prefix";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Prefix", strType + "%");
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var idString = reader.GetString(0);
                    var parts = idString.Split('-');
                    if (parts.Length == 2
                        && int.TryParse(parts[1], out var num)
                        && num > maxNumber)
                        maxNumber = num;
                }
            }
        }

        return $"{strType}{maxNumber + 1:D2}";
    }

    public IEnumerable<DeviceDto> GetAllDevices()
    {
        var devices = new List<DeviceDto>();
        const string sql = "SELECT * FROM Device";

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    devices.Add(new DeviceDto
                    {
                        Id = reader.GetString(0),
                        Name = reader.GetString(1),
                        IsEnabled = reader.GetBoolean(2),
                        RowVersion = reader.GetSqlBinary(3).Value
                    });
            }
        }

        return devices;
    }
    public DeviceDto? GetDeviceById(string id)
    {
        var querystring = "SELECT * FROM Device WHERE Id = @id";

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(querystring, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new DeviceDto
                    {
                        Id = reader.GetString(0),
                        Name = reader.GetString(1),
                        IsEnabled = reader.GetBoolean(2),
                        RowVersion = reader.GetSqlBinary(3).Value
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving device: {ex.Message}");
            return null;
        }
    }

    public SmartwatchDto? GetSmartwatchById(string id)
    {
        SmartwatchDto smartwatch = null;
        const string sql = @"
        SELECT d.Id, d.Name, d.IsEnabled, d.RowVersion, s.BatteryPercentage
        FROM Device d
        JOIN Smartwatch s ON d.Id = s.DeviceId
        WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        smartwatch = new SmartwatchDto
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsEnabled = reader.GetBoolean(2),
                            BatteryLevel = reader.GetInt32(4),
                            RowVersion = reader.GetSqlBinary(3).Value
                        };

                    }
                }
            }
        }
        return smartwatch;
    }
    
    public PersonalComputerDto? GetPersonalComputerById(string id)
    {
        PersonalComputerDto pc = null;
        const string sql = @"
        SELECT d.Id, d.Name, d.IsEnabled, d.RowVersion, p.OperationSystem
        FROM Device d
        JOIN PersonalComputer p ON d.Id = p.DeviceId
        WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Correctly mapping to the DTO
                        pc = new PersonalComputerDto()
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsEnabled = reader.GetBoolean(2),
                            OperatingSystem = reader.GetString(4),
                            RowVersion = reader.GetSqlBinary(3).Value
                        };

                    }
                }
            }
        }
        return pc;
    }
    public EmbeddedDto? GetEmbeddedDevicesById(string id)
    {
        EmbeddedDto? edDto = null;
        const string sql = @"
        SELECT d.Id, d.Name, d.IsEnabled, d.RowVersion, e.IpAddress, e.NetworkName
        FROM Device d
        JOIN Embedded e ON d.Id = e.DeviceId
        WHERE d.Id = @id";

        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        edDto = new EmbeddedDto
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsEnabled = reader.GetBoolean(2),
                            RowVersion = reader.GetSqlBinary(3).Value,
                            IpAddress = reader.GetString(4),
                            NetworkName = reader.GetString(5)
                        };
                    }
                }
            }
        }

        return edDto;
    }
    
    public bool AddSmartwatch(Smartwatches device)
    {
        var swId = GetIntId(device);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand("AddSmartwatch", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SwId", swId);
                        cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                        cmd.Parameters.AddWithValue("@Name", device.Name);
                        cmd.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                        cmd.Parameters.AddWithValue("@BatteryPercentage", device.BatteryPercentage);
                        cmd.ExecuteNonQuery();
                    }

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
    }

    public bool AddPersonalComputer(PersonalComputer device)
    {
        var pcId = GetIntId(device);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand("AddPersonalComputer", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PcId", pcId);
                        cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                        cmd.Parameters.AddWithValue("@Name", device.Name);
                        cmd.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                        cmd.Parameters.AddWithValue("@OperationSystem", device.OperatingSystem);
                        cmd.ExecuteNonQuery();
                    }

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
    }

    public bool AddEmbedded(EmbeddedDevices device)
    {
        var edId = GetIntId(device);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand("AddEmbedded", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EdId", edId);
                        cmd.Parameters.AddWithValue("@DeviceId", device.Id);
                        cmd.Parameters.AddWithValue("@Name", device.Name);
                        cmd.Parameters.AddWithValue("@IsEnabled", device.IsOn);
                        cmd.Parameters.AddWithValue("@IpAddress", device.IpName);
                        cmd.Parameters.AddWithValue("@NetworkName", device.NetworkName);
                        cmd.ExecuteNonQuery();
                    }

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
    }

    public bool DeleteSmartwatch(string id)
    {
        const string deleteSw = "DELETE FROM Smartwatch WHERE DeviceId = @Id";
        const string deleteDevice = "DELETE FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var swCmd = new SqlCommand(deleteSw, connection, transaction))
                    {
                        swCmd.Parameters.AddWithValue("@Id", id);
                        swCmd.ExecuteNonQuery();
                    }

                    using (var devCmd = new SqlCommand(deleteDevice, connection, transaction))
                    {
                        devCmd.Parameters.AddWithValue("@Id", id);
                        devCmd.ExecuteNonQuery();
                    }

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
    }


    public bool DeletePersonalComputer(string id)
    {
        const string deletePC = "DELETE FROM PersonalComputer WHERE DeviceId = @Id";
        const string deleteDevice = "DELETE FROM Device WHERE Id = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var pcCmd = new SqlCommand(deletePC, connection, transaction))
                    {
                        pcCmd.Parameters.AddWithValue("@Id", id);
                        pcCmd.ExecuteNonQuery();
                    }

                    using (var devCmd = new SqlCommand(deleteDevice, connection, transaction))
                    {
                        devCmd.Parameters.AddWithValue("@Id", id);
                        devCmd.ExecuteNonQuery();
                    }

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
    }

    public bool DeleteEmbeddedDevice(string id)
    {
        const string deleteED = "DELETE FROM Embedded WHERE DeviceId = @Id";
        const string deleteDevice = "DELETE FROM Device WHERE Id       = @Id";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var edCmd = new SqlCommand(deleteED, connection, transaction))
                    {
                        edCmd.Parameters.AddWithValue("@Id", id);
                        edCmd.ExecuteNonQuery();
                    }

                    using (var devCmd = new SqlCommand(deleteDevice, connection, transaction))
                    {
                        devCmd.Parameters.AddWithValue("@Id", id);
                        devCmd.ExecuteNonQuery();
                    }

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
    }

    public bool DeviceExists(string id)
    {
        const string sql = "SELECT COUNT(*) FROM Device WHERE Id = @Id";
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
    

    public bool UpdateDevice(DeviceDto deviceDto)
    {
        const string sqlUpdateDevice = @"UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled 
                                    WHERE Id = @Id AND RowVersion = @RowVersion";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = new SqlCommand(sqlUpdateDevice, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@DeviceId", deviceDto.Id);
                        command.Parameters.AddWithValue("@DeviceName", deviceDto.Name);
                        command.Parameters.AddWithValue("@IsActive", deviceDto.IsEnabled);
                        command.Parameters.AddWithValue("@RowVersion", deviceDto.RowVersion);

                        var affectedRows = command.ExecuteNonQuery();

                        if (affectedRows == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    var result = deviceDto switch
                    {
                        SmartwatchDto swDto => UpdateSmartwatch(swDto, connection, transaction),
                        PersonalComputerDto pcDto => UpdatePersonalComputer(pcDto, connection, transaction),
                        EmbeddedDto edDto => UpdateEmbedded(edDto, connection, transaction),
                        _ => false
                    };

                    if (!result)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error updating device: {ex.Message}");
                    return false;
                }
            }
        }
    }

    public bool UpdateSmartwatch(SmartwatchDto swDto, SqlConnection connection, SqlTransaction transaction)
    {
        const string sqlUpdateSmartwatch = @"
        UPDATE Smartwatch
        SET BatteryPercentage = @BatteryPercentage
        WHERE DeviceId = @DeviceId";

        using (var command = new SqlCommand(sqlUpdateSmartwatch, connection, transaction))
        {
            command.Parameters.AddWithValue("@DeviceId", swDto.Id);
            command.Parameters.AddWithValue("@BatteryPercentage", swDto.BatteryLevel);
            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public bool UpdatePersonalComputer(PersonalComputerDto pcDto, SqlConnection connection, SqlTransaction transaction)
    {
        const string sqlUpdatePC = @"
        UPDATE PersonalComputer
        SET OperatingSystem = @OperatingSystem
        WHERE DeviceId = @DeviceId";

        using (var command = new SqlCommand(sqlUpdatePC, connection, transaction))
        {
            command.Parameters.AddWithValue("@DeviceId", pcDto.Id);
            command.Parameters.AddWithValue("@OperatingSystem", pcDto.OperatingSystem);
            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public bool UpdateEmbedded(EmbeddedDto edDto, SqlConnection connection, SqlTransaction transaction)
    {
        const string sqlUpdateED = @"
        UPDATE Embedded
        SET IpAddress = @IpAddress, NetworkName = @NetworkName
        WHERE DeviceId = @DeviceId";

        using (var command = new SqlCommand(sqlUpdateED, connection, transaction))
        {
            command.Parameters.AddWithValue("@DeviceId", edDto.Id);
            command.Parameters.AddWithValue("@IpAddress", edDto.IpAddress);
            command.Parameters.AddWithValue("@NetworkName", edDto.NetworkName);
            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }


    private static int GetIntId(Device device)
    {
        return int.Parse(device.Id.Split('-')[1]);
    }
}