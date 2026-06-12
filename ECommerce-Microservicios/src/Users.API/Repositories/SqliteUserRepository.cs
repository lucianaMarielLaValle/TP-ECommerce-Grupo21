using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Repositories;

public class SqliteUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public SqliteUserRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<User> AddAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO users (Id, Nombre, Apellido, Email, FechaRegistro, Activo, PasswordHash, IntentosFallidos, MotivoBloqueo)
            VALUES (@Id, @Nombre, @Apellido, @Email, @FechaRegistro, @Activo, @PasswordHash, @IntentosFallidos, @MotivoBloqueo);
            """, user);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE LOWER(Email) = LOWER(@Email)",
            new { Email = email });
    }

    public async Task<User> UpdateAsync(User user)
    {
        using var conn = CreateConnection();
        var rowsAffected = await conn.ExecuteAsync("""
            UPDATE users
            SET Nombre = @Nombre,
                Apellido = @Apellido,
                Email = @Email,
                FechaRegistro = @FechaRegistro,
                Activo = @Activo,
                PasswordHash = @PasswordHash,
                IntentosFallidos = @IntentosFallidos,
                MotivoBloqueo = @MotivoBloqueo
            WHERE Id = @Id;
            """, user);

        if (rowsAffected == 0)
            throw new NotFoundException("USR-006", "Usuario no encontrado para actualizar.");

        return user;
    }
}
