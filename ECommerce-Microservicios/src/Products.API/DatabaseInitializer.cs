using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                Id TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Descripcion TEXT,
                Precio REAL NOT NULL DEFAULT 0,
                Stock INTEGER NOT NULL DEFAULT 0,
                Categoria TEXT NOT NULL,
                FechaCreacion TEXT NOT NULL
            );
        """);
    }
}