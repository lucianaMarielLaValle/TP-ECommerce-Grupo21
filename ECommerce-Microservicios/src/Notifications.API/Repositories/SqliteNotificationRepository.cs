using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Repositories;

public class SqliteNotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public SqliteNotificationRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<Notification> AddAsync(Notification notification)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO notifications (Id, UsuarioId, Mensaje, Tipo, Estado, FechaEnvio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio);
            """, notification);
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Notification>(
            "SELECT * FROM notifications WHERE UsuarioId = @UserId ORDER BY FechaEnvio DESC",
            new { UserId = userId });
    }
}
