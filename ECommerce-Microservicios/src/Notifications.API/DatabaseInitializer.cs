using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                Id TEXT PRIMARY KEY,
                UsuarioId TEXT NOT NULL,
                Mensaje TEXT NOT NULL,
                Tipo TEXT NOT NULL,
                Estado TEXT NOT NULL,
                FechaEnvio TEXT NOT NULL
            );
        """);
    }
}
