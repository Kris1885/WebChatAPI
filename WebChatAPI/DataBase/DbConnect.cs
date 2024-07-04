using Microsoft.Data.Sqlite;

namespace WebChatAPI.DataBase;

public class DbConnect : IDisposable
{
    private SqliteConnection connection;

    private DbConnect()
    {
    }

    public static DbConnect New()
    {
        var inst = new DbConnect();
        inst.connection = inst.InitAndCheckDataBase();
        return inst;
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    public int Execute(string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteNonQuery();
    }

    public DataReader ExecuteAndRead(string sql)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        var reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            return new DataReader(null)
            {
                HasData = false,
            };
        }

        reader.Read();
        return new DataReader(reader)
        {
            HasData = true,
        };
    }

    private SqliteConnection InitAndCheckDataBase()
    {
        string path = $"{Environment.CurrentDirectory}\\database.db";
        bool isExist = System.IO.File.Exists(path);
        var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        if (connection.State != System.Data.ConnectionState.Open)
            throw new InvalidOperationException("Не удалось подключиться к базе данных");

        if (!isExist)
        {
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Messages(Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, Message TEXT NOT NULL, UserId INTEGER NOT NULL)";
            command.ExecuteNonQuery();

            var commnad2 = connection.CreateCommand();
            commnad2.CommandText = "CREATE TABLE Users(Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, " +
                "Name TEXT NOT NULL, " +
                "Login TEXT NOT NULL UNIQUE, " +
                " Password TEXT NOT NULL)";
            commnad2.ExecuteNonQuery();

            var commandCreateTokens = connection.CreateCommand();
            commandCreateTokens.CommandText = "CREATE TABLE Tokens(Token TEXT NOT NULL UNIQUE, UserId INTEGER NOT NULL)";
            commandCreateTokens.ExecuteNonQuery();
        }

        return connection;
    }
}

public class DataReader
{
    private readonly SqliteDataReader _reader;

    /// <summary>
    /// Не вызывайте этот экземпляр сами!
    /// </summary>
    public DataReader(SqliteDataReader reader)
    {
        _reader = reader;
    }

    public bool HasData { get; internal set; }

    public int GetInt32(int columnId)
    {
        return _reader.GetInt32(columnId);
    }

    public string GetString(int columnId)
    {
        return _reader.GetString(columnId);
    }

    public Task<bool> NextRow()
    {
        return _reader.ReadAsync();
    }
}