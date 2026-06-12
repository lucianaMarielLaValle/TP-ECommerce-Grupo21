using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                Id TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Apellido TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                FechaRegistro TEXT NOT NULL,
                Activo INTEGER NOT NULL,
                PasswordHash TEXT NOT NULL,
                IntentosFallidos INTEGER NOT NULL DEFAULT 0,
                MotivoBloqueo TEXT NOT NULL DEFAULT ''
            );
        """);
    }
}
