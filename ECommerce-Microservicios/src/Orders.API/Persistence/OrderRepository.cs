using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.Api.Models;

namespace Orders.Api.Persistence;

/// <summary>
/// Implementación del repositorio de órdenes con SQLite + Dapper.
/// Los Guid y las fechas se persisten como TEXT para un mapeo explícito y sin sorpresas.
/// El total y el precio unitario se guardan como REAL (suficiente para el alcance académico;
/// para producción se recomendaría INTEGER en centavos).
/// </summary>
public class OrderRepository(IConfiguration config, ILogger<OrderRepository> logger) : IOrderRepository
{
    private readonly string _connectionString =
        config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    // Formato de fecha redondeable (ISO 8601) para round-trip exacto al leer/escribir.
    private const string FechaFormato = "o";

    public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var sql = """
            SELECT id, usuario_id, total, estado, fecha_creacion
            FROM orders
            """;
        if (usuarioId.HasValue)
            sql += " WHERE usuario_id = @UsuarioId";
        sql += " ORDER BY fecha_creacion DESC";

        var filas = await conn.QueryAsync<OrderRow>(
            sql, new { UsuarioId = usuarioId?.ToString() });

        var ordenes = new List<Order>();
        foreach (var fila in filas)
        {
            var orden = MapOrder(fila);
            orden.Items = await CargarItemsAsync(conn, orden.Id);
            ordenes.Add(orden);
        }

        logger.LogInformation("Se recuperaron {Cantidad} órdenes.", ordenes.Count);
        return ordenes;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var fila = await conn.QuerySingleOrDefaultAsync<OrderRow>(
            """
            SELECT id, usuario_id, total, estado, fecha_creacion
            FROM orders
            WHERE id = @Id
            """,
            new { Id = id.ToString() });

        if (fila is null)
            return null;

        var orden = MapOrder(fila);
        orden.Items = await CargarItemsAsync(conn, orden.Id);
        return orden;
    }

    public async Task CreateAsync(Order order)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(
            """
            INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
            VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
            """,
            new
            {
                Id = order.Id.ToString(),
                UsuarioId = order.UsuarioId.ToString(),
                Total = (double)order.Total,
                order.Estado,
                FechaCreacion = order.FechaCreacion.ToString(FechaFormato, CultureInfo.InvariantCulture)
            },
            tx);

        foreach (var item in order.Items)
        {
            await conn.ExecuteAsync(
                """
                INSERT INTO order_items (orden_id, producto_id, cantidad, precio_unitario)
                VALUES (@OrdenId, @ProductoId, @Cantidad, @PrecioUnitario)
                """,
                new
                {
                    OrdenId = order.Id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad,
                    PrecioUnitario = (double)item.PrecioUnitario
                },
                tx);
        }

        tx.Commit();
        logger.LogInformation("Orden {OrdenId} creada con {Items} item(s).", order.Id, order.Items.Count);
    }

    public async Task UpdateEstadoAsync(Guid id, string estado)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        await conn.ExecuteAsync(
            "UPDATE orders SET estado = @Estado WHERE id = @Id",
            new { Id = id.ToString(), Estado = estado });

        logger.LogInformation("Orden {OrdenId} actualizada al estado {Estado}.", id, estado);
    }

    // ---- Helpers de mapeo ----

    private static async Task<List<OrderItem>> CargarItemsAsync(SqliteConnection conn, Guid ordenId)
    {
        var filas = await conn.QueryAsync<ItemRow>(
            """
            SELECT producto_id, cantidad, precio_unitario
            FROM order_items
            WHERE orden_id = @OrdenId
            """,
            new { OrdenId = ordenId.ToString() });

        return filas.Select(f => new OrderItem
        {
            ProductoId = Guid.Parse(f.producto_id),
            Cantidad = f.cantidad,
            PrecioUnitario = (decimal)f.precio_unitario
        }).ToList();
    }

    private static Order MapOrder(OrderRow fila) => new()
    {
        Id = Guid.Parse(fila.id),
        UsuarioId = Guid.Parse(fila.usuario_id),
        Total = (decimal)fila.total,
        Estado = fila.estado,
        FechaCreacion = DateTime.Parse(
            fila.fecha_creacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
    };

    // Registros internos que reflejan las columnas crudas de la base.
    private sealed record OrderRow(string id, string usuario_id, double total, string estado, string fecha_creacion);
    private sealed record ItemRow(string producto_id, int cantidad, double precio_unitario);
}
