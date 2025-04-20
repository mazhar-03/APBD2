using DeviceManager.Entities;
using Microsoft.Data.SqlClient;

namespace DeviceManager.Logic;

public class MsSqlDeviceRepository : IDeviceRepository
{
    private readonly string _connectionString;

    public MsSqlDeviceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IEnumerable<Smartwatches> GetAllSmartwatches()
    {
        var smartwatches = new List<Smartwatches>();
        string sql = "SELECT * FROM Smartwatches";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var smartwatch = new Smartwatches
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2),
                            reader.GetInt32(3)
                        );
                        smartwatches.Add(smartwatch);
                    }
            }
            finally
            {
                reader.Close();
            }
        }
        return smartwatches;
    }

    public bool AddSmartwatch(Smartwatches device)
    {
        var insert =
            "INSERT INTO Smartwatches (Id, Name, IsOn, BatteryPercentage) VALUES (@Id, @Name, @IsOn, @BatteryPercentage)";
        
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(insert, connection);
            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@BatteryPercentage", device.BatteryPercentage);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool UpdateSmartwatch(string id, Smartwatches device)
    {
        var update = 
            "UPDATE Smartwatches SET Name = @Name, IsOn = @IsOn, BatteryPercentage = @BatteryPercentage WHERE Id = @Id"; 
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(update, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@BatteryPercentage", device.BatteryPercentage);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool DeleteSmartwatch(string id)
    {
        var delete = "DELETE FROM Smartwatches WHERE Id = @Id";
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public IEnumerable<PersonalComputer> GetAllPersonalComputers()
    {
        var pcs = new List<PersonalComputer>();
        string sql = "SELECT * FROM PersonalComputers";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var pc = new PersonalComputer
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2),
                            reader.GetString(3)
                        );
                        pcs.Add(pc);
                    }
            }
            finally
            {
                reader.Close();
            }
        }
        return pcs;
    }

    public bool AddPersonalComputer(PersonalComputer device)
    {
        var insert =
            "INSERT INTO PersonalComputers (Id, Name, IsOn, OperatingSystem) VALUES (@Id, @Name, @IsOn, @OperatingSystem)";
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(insert, connection);
            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@OperatingSystem", device.OperatingSystem);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool UpdatePersonalComputer(string id, PersonalComputer device)
    {
        var update = 
            "UPDATE PersonalComputers SET Name = @Name, IsOn = @IsOn, OperatingSystem = @OperatingSystem WHERE Id = @Id"; 
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(update, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@BatteryPercentage", device.OperatingSystem);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool DeletePersonalComputer(string id)
    {
        var delete = "DELETE FROM PersonalComputers WHERE Id = @Id";
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public IEnumerable<EmbeddedDevices> GetAllEmbeddedDevices()
    {
        var eds = new List<EmbeddedDevices>();
        string sql = "SELECT * FROM EmbeddedDevices";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var ed = new EmbeddedDevices
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        );
                        eds.Add(ed);
                    }
            }
            finally
            {
                reader.Close();
            }
        }
        return eds;
    }

    public bool AddEmbeddedDevice(EmbeddedDevices device)
    {
        var insert =
            "INSERT INTO EmbeddedDevices (Id, Name, IsOn, IpName, NetworkName) VALUES (@Id, @Name, @IsOn, @IPName, @NetworkName))";
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(insert, connection);
            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@IpName", device.IpName);
            command.Parameters.AddWithValue("@NetworkName", device.NetworkName);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool UpdateEmbeddedDevice(string id, EmbeddedDevices device)
    {
        var update = 
            "UPDATE EmbeddedDevices SET Name = @Name, IsOn = @IsOn, IpName = @IpName, NetworkName = @NetworkName WHERE Id = @Id"; 
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(update, connection);
            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@IpName", device.IpName);
            command.Parameters.AddWithValue("@NetworkName", device.NetworkName);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }

    public bool DeleteEmbeddedDevice(string id)
    {
        var delete = "DELETE FROM EmbeddedDevices WHERE Id = @Id";
        int rowsAffected = 0;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }
        return rowsAffected > 0;
    }
}