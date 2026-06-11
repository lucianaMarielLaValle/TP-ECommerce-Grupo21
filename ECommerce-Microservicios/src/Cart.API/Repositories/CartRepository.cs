using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;

namespace Cart.API.Repositories;

/// <summary>
/// Repositorio de carritos 
/// </summary>
public class CartRepository(IConfiguration config, ILogger<CartRepository> logger) : ICartRepository
{
    private readonly string _connectionString =
        config.GetConnectionString("DefaultConnection") ?? "Data Source=cart.db";

    private const string FechaFormato = "o";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<Models.Cart?> GetByUsuarioIdAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var fila = await conn.QuerySingleOrDefaultAsync<CartRow>(
            """
            SELECT usuario_id, fecha_actualizacion
            FROM carts
            WHERE usuario_id = @UsuarioId
            """,
            new { UsuarioId = usuarioId.ToString() });

        if (fila is null)
            return null;

        var items = await conn.QueryAsync<ItemRow>(
            """
            SELECT producto_id, cantidad
            FROM cart_items
            WHERE usuario_id = @UsuarioId
            """,
            new { UsuarioId = usuarioId.ToString() });

        return new Models.Cart
        {
            UsuarioId = Guid.Parse(fila.usuario_id),
            FechaActualizacion = DateTime.Parse(
                fila.fecha_actualizacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Items = items.Select(i => new CartItem
            {
                ProductoId = Guid.Parse(i.producto_id),
                Cantidad = (int)i.cantidad
            }).ToList()
        };
    }

    public async Task SaveAsync(Models.Cart cart)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        // Upsert de la cabecera del carrito.
        await conn.ExecuteAsync(
            """
            INSERT INTO carts (usuario_id, fecha_actualizacion)
            VALUES (@UsuarioId, @Fecha)
            ON CONFLICT(usuario_id) DO UPDATE SET fecha_actualizacion = excluded.fecha_actualizacion
            """,
            new
            {
                UsuarioId = cart.UsuarioId.ToString(),
                Fecha = cart.FechaActualizacion.ToString(FechaFormato, CultureInfo.InvariantCulture)
            },
            tx);

        // Reescribe los items: borra los actuales y reinserta el estado nuevo.
        await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @UsuarioId",
            new { UsuarioId = cart.UsuarioId.ToString() }, tx);

        foreach (var item in cart.Items)
        {
            await conn.ExecuteAsync(
                """
                INSERT INTO cart_items (usuario_id, producto_id, cantidad)
                VALUES (@UsuarioId, @ProductoId, @Cantidad)
                """,
                new
                {
                    UsuarioId = cart.UsuarioId.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad
                },
                tx);
        }

        tx.Commit();
        logger.LogInformation("Carrito del usuario {UsuarioId} guardado con {Items} item(s).",
            cart.UsuarioId, cart.Items.Count);
    }

    public async Task DeleteAsync(Guid usuarioId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("DELETE FROM cart_items WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, tx);
        await conn.ExecuteAsync("DELETE FROM carts WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, tx);

        tx.Commit();
        logger.LogInformation("Carrito del usuario {UsuarioId} eliminado.", usuarioId);
    }

    private sealed record CartRow(string usuario_id, string fecha_actualizacion);
    private sealed record ItemRow(string producto_id, long cantidad);
}
