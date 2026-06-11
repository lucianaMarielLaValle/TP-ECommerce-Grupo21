using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var conn = CreateConnection();
        var sql = "SELECT * FROM products WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(categoria))
        {
            sql += " AND LOWER(Categoria) = LOWER(@Categoria)";
            parameters.Add("Categoria", categoria);
        }
        if (!string.IsNullOrEmpty(nombre))
        {
            sql += " AND LOWER(Nombre) LIKE LOWER(@Nombre)";
            parameters.Add("Nombre", $"%{nombre}%");
        }
        sql += " ORDER BY FechaCreacion DESC";

        return await conn.QueryAsync<Product>(sql, parameters);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM products WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> ExisteDuplicadoAsync(string nombre, string categoria)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM products WHERE LOWER(Nombre) = LOWER(@Nombre) AND LOWER(Categoria) = LOWER(@Categoria)",
            new { Nombre = nombre, Categoria = categoria });
        return count > 0;
    }

    public async Task CreateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO products (Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion);
        """, product);
    }

    public async Task UpdateAsync(Product product)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE products
            SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio,
                Stock = @Stock, Categoria = @Categoria
            WHERE Id = @Id;
        """, product);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("DELETE FROM products WHERE Id = @Id", new { Id = id });
    }
}