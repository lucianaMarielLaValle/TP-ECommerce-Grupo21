using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Repositories;


public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    private readonly string _connectionString =
        config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        connection.Execute("PRAGMA foreign_keys = ON;");

        connection.Execute("""
        CREATE TABLE IF NOT EXISTS carts (
            usuario_id          TEXT PRIMARY KEY,
            fecha_actualizacion TEXT NOT NULL
        );
        """);

        connection.Execute("""
        CREATE TABLE IF NOT EXISTS cart_items (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            usuario_id  TEXT    NOT NULL,
            producto_id TEXT    NOT NULL,
            cantidad    INTEGER NOT NULL,
            FOREIGN KEY (usuario_id) REFERENCES carts(usuario_id)
        );
        """);

        logger.LogInformation("Base de datos de Cart inicializada correctamente.");
    }
}
