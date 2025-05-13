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
                    });
            }
        }

        return devices;
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
                        // Correctly mapping to the DTO
                        smartwatch = new SmartwatchDto
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsEnabled = reader.GetBoolean(2),
                            BatteryLevel = reader.GetInt32(3),
                            RowVersion = [reader.GetByte(4)]
                        };

                    }
                }
            }
        }
        return smartwatch;
    }

    public bool UpdateSmartwatch(string id, SmartwatchDto device)
    {
        const string updateDevice = @"
        UPDATE Device
        SET Name = @Name, IsEnabled = @IsEnabled
        WHERE Id = @Id AND RowVersion = @RowVersion";
        
        const string updateSw = @"
                UPDATE Smartwatch
                SET BatteryPercentage = @BatteryPercentage
                WHERE DeviceId = @DeviceId";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var deviceCmd = new SqlCommand(updateDevice, connection, transaction))
                    {
                        deviceCmd.Parameters.AddWithValue("@Id", id);
                        deviceCmd.Parameters.AddWithValue("@Name", device.Name);
                        deviceCmd.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
                        deviceCmd.Parameters.Add("@RowVersion", SqlDbType.Timestamp).Value = device.RowVersion; 

                        int rowsAffected = deviceCmd.ExecuteNonQuery();

                        // if no rows affected, must be a conflict.
                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return false; 
                        }
                    }

                    using (var swCmd = new SqlCommand(updateSw, connection, transaction))
                    {
                        swCmd.Parameters.AddWithValue("@BatteryPercentage", device.BatteryLevel);
                        swCmd.Parameters.AddWithValue("@DeviceId", device.Id);
                        swCmd.ExecuteNonQuery();
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
                            OperatingSystem = reader.GetString(3),
                            RowVersion = [reader.GetByte(4)]
                        };

                    }
                }
            }
        }
        return pc;
    }

    public bool UpdatePersonalComputer(string id, PersonalComputerDto device)
{
    const string updateDevice = @"
        UPDATE Device
        SET Name = @Name, IsEnabled = @IsEnabled
        WHERE Id = @Id AND RowVersion = @RowVersion";  

    const string updatePC = @"
        UPDATE PersonalComputer
        SET OperationSystem = @OperationSystem
        WHERE DeviceId = @DeviceId";

    using (var connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                using (var deviceCmd = new SqlCommand(updateDevice, connection, transaction))
                {
                    deviceCmd.Parameters.AddWithValue("@Id", id);
                    deviceCmd.Parameters.AddWithValue("@Name", device.Name);
                    deviceCmd.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
                    deviceCmd.Parameters.Add("@RowVersion", SqlDbType.Timestamp).Value = device.RowVersion; 

                    int rowsAffected = deviceCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;  
                    }
                }

                using (var pcCmd = new SqlCommand(updatePC, connection, transaction))
                {
                    pcCmd.Parameters.AddWithValue("@OperationSystem", device.OperatingSystem);
                    pcCmd.Parameters.AddWithValue("@DeviceId", device.Id);
                    pcCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
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
                            RowVersion = [reader.GetByte(4)],
                            IpAddress = reader.GetString(3),
                            NetworkName = reader.GetString(5)
                        };
                    }
                }
            }
        }

        return edDto;
    }
    
    public bool UpdateEmbeddedDevice(string id, EmbeddedDto device)
{
    const string updateDevice = @"
        UPDATE Device
        SET Name = @Name, IsEnabled = @IsEnabled
        WHERE Id = @Id AND RowVersion = @RowVersion";  

    const string updateED = @"
        UPDATE Embedded
        SET IpAddress = @IpAddress, NetworkName = @NetworkName
        WHERE DeviceId = @DeviceId";

    using (var connection = new SqlConnection(_connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                using (var deviceCmd = new SqlCommand(updateDevice, connection, transaction))
                {
                    deviceCmd.Parameters.AddWithValue("@Id", id);
                    deviceCmd.Parameters.AddWithValue("@Name", device.Name);
                    deviceCmd.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
                    deviceCmd.Parameters.Add("@RowVersion", SqlDbType.Timestamp).Value = device.RowVersion; 

                    int rowsAffected = deviceCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;  
                    }
                }
                using (var edCmd = new SqlCommand(updateED, connection, transaction))
                {
                    edCmd.Parameters.AddWithValue("@IpAddress", device.IpAddress);
                    edCmd.Parameters.AddWithValue("@NetworkName", device.NetworkName);
                    edCmd.Parameters.AddWithValue("@DeviceId", device.Id);
                    edCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}"); 
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

    private static int GetIntId(Device device)
    {
        return int.Parse(device.Id.Split('-')[1]);
    }
}