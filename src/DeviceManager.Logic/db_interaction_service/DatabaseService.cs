using DeviceManager.Entities;
using Microsoft.Data.SqlClient;

namespace DeviceManager.Logic;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Smartwatches> GetAllSmartwatches()
    {
        var smartwatches = new List<Smartwatches>();
        var sql = "SELECT * FROM Smartwatches";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
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

    public Smartwatches GetSmartwatchById(string id)
    {
        Smartwatches smartwatch = null;
        var sql = "SELECT * FROM Smartwatches WHERE id=@id";

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
        var insert =
            "INSERT INTO Smartwatches (Id, Name, IsOn, BatteryPercentage) VALUES (@Id, @Name, @IsOn, @BatteryPercentage)";

        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(insert, connection);
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
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(update, connection);
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
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }

        return rowsAffected > 0;
    }

    public IEnumerable<PersonalComputer> GetAllPersonalComputers()
    {
        var pcs = new List<PersonalComputer>();
        var sql = "SELECT * FROM PersonalComputers";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
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

    public PersonalComputer GetPersonalComputerById(string id)
    {
        PersonalComputer pc = null;
        var sql = "SELECT * FROM PersonalComputers WHERE id=@id";

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
                        pc = new PersonalComputer
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetBoolean(2),
                            reader.GetString(3)
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
        var insert =
            "INSERT INTO PersonalComputers (Id, Name, IsOn, OperatingSystem) VALUES (@Id, @Name, @IsOn, @OperatingSystem)";
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(insert, connection);
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
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(update, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsOn", device.IsOn);
            command.Parameters.AddWithValue("@OperatingSystem", device.OperatingSystem);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }

        return rowsAffected > 0;
    }

    public bool DeletePersonalComputer(string id)
    {
        var delete = "DELETE FROM PersonalComputers WHERE Id = @Id";
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }

        return rowsAffected > 0;
    }

    public IEnumerable<EmbeddedDevices> GetAllEmbeddedDevices()
    {
        var eds = new List<EmbeddedDevices>();
        var sql = "SELECT * FROM EmbeddedDevices";

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(sql, connection);
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

    public EmbeddedDevices GetEmbeddedDevicesById(string id)
    {
        EmbeddedDevices ed = null;
        var sql = "SELECT * FROM EmbeddedDevices WHERE id=@id";

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
                        ed = new EmbeddedDevices
                        (
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
        var insert =
            "INSERT INTO EmbeddedDevices (Id, Name, IsOn, IpName, NetworkName) VALUES (@Id, @Name, @IsOn, @IPName, @NetworkName)";
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(insert, connection);
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
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(update, connection);
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
        var rowsAffected = 0;

        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(delete, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            rowsAffected = command.ExecuteNonQuery();
        }

        return rowsAffected > 0;
    }
}