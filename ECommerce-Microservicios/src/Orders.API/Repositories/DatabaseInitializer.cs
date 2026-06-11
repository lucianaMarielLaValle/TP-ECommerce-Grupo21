using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Repositories;

/// <summary>
/// Crea el esquema de la base SQLite al arrancar la aplicación.
/// Sigue el patrón "CREATE TABLE IF NOT EXISTS" de la Guía de Componentes (MiniApi).
/// Se invoca una vez desde Program.cs durante el arranque.
/// </summary>
public class DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
{
    private readonly string _connectionString =
        config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Habilita la verificación de claves foráneas en SQLite.
        connection.Execute("PRAGMA foreign_keys = ON;");

        connection.Execute("""
        CREATE TABLE IF NOT EXISTS orders (
            id             TEXT    PRIMARY KEY,
            usuario_id     TEXT    NOT NULL,
            total          REAL    NOT NULL DEFAULT 0,
            estado         TEXT    NOT NULL,
            fecha_creacion TEXT    NOT NULL
        );
        """);

        connection.Execute("""
        CREATE TABLE IF NOT EXISTS order_items (
            id              INTEGER PRIMARY KEY AUTOINCREMENT,
            orden_id        TEXT    NOT NULL,
            producto_id     TEXT    NOT NULL,
            cantidad        INTEGER NOT NULL,
            precio_unitario REAL    NOT NULL,
            FOREIGN KEY (orden_id) REFERENCES orders(id)
        );
        """);

        logger.LogInformation("Base de datos de Orders inicializada correctamente.");
    }
}
