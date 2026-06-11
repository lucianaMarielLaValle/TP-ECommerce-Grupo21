using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;
using Orders.API.Repositories;

namespace Orders.API.Services;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ProductsClient _productsClient;
    private readonly UsersClient _usersClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ProductsClient productsClient,
        UsersClient usersClient,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _productsClient = productsClient;
        _usersClient = usersClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
    {
        _logger.LogInformation("Listando órdenes. Filtro usuarioId={UsuarioId}", usuarioId);
        return await _repository.GetAllAsync(usuarioId);
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Orden no encontrada. ErrorCode={ErrorCode}, Id={Id}", "ORD-001", id);
            throw new NoEncontradoException("ORD-001", "Orden no encontrada.");
        }
        return order;
    }

    public async Task<Order> CreateAsync(CrearOrdenDTO request)
    {
        // 1. Datos básicos (ORD-002)
        if (request.Items == null || request.Items.Count == 0
            || request.Items.Any(i => i.Cantidad <= 0))
        {
            _logger.LogWarning("Orden inválida. ErrorCode={ErrorCode}", "ORD-002");
            throw new ValidacionException("ORD-002", "Los datos de la orden son inválidos.");
        }

        // 2. Usuario existe (ORD-003)
        if (!await _usersClient.ExisteUsuarioAsync(request.UsuarioId))
        {
            _logger.LogWarning("Usuario no encontrado. ErrorCode={ErrorCode}, UsuarioId={UsuarioId}", "ORD-003", request.UsuarioId);
            throw new NoEncontradoException("ORD-003", "Usuario no encontrado al crear la orden.");
        }

        // 3 y 4. Productos y stock; armado de items
        var items = new List<OrderItem>();
        decimal total = 0;

        foreach (var itemReq in request.Items)
        {
            var producto = await _productsClient.ObtenerProductoAsync(itemReq.ProductoId);

            // 3. Producto existe (ORD-004)
            if (producto == null)
            {
                _logger.LogWarning("Producto no encontrado. ErrorCode={ErrorCode}, ProductoId={ProductoId}", "ORD-004", itemReq.ProductoId);
                throw new NoEncontradoException("ORD-004", "Producto no encontrado al crear la orden.");
            }

            // 4. Stock suficiente (ORD-005)
            if (producto.Stock < itemReq.Cantidad)
            {
                _logger.LogWarning("Stock insuficiente. ErrorCode={ErrorCode}, ProductoId={ProductoId}", "ORD-005", itemReq.ProductoId);
                throw new ReglaNegocioException("ORD-005",
                    $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {itemReq.Cantidad}.");
            }

            // 5 y 6. Capturar precio unitario y acumular total
            items.Add(new OrderItem
            {
                ProductoId = producto.Id,
                Cantidad = itemReq.Cantidad,
                PrecioUnitario = producto.Precio
            });
            total += producto.Precio * itemReq.Cantidad;
        }

        // 7. Crear la orden en estado Pendiente
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = items,
            Total = total,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.CreateAsync(order);
        _logger.LogInformation("Orden creada. Id={Id}, Total={Total}", order.Id, order.Total);
        return order;
    }

    public async Task<Order> CambiarEstadoAsync(Guid id, CambiarEstadoDTO request)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Orden no encontrada. ErrorCode={ErrorCode}, Id={Id}", "ORD-001", id);
            throw new NoEncontradoException("ORD-001", "Orden no encontrada.");
        }

        var estadosValidos = new[] { "Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada" };
        if (string.IsNullOrWhiteSpace(request.Estado) || !estadosValidos.Contains(request.Estado))
        {
            _logger.LogWarning("Estado inválido. ErrorCode={ErrorCode}, Estado={Estado}", "ORD-002", request.Estado);
            throw new ValidacionException("ORD-002", "El estado indicado no es válido.");
        }

        if (!EsTransicionValida(order.Estado, request.Estado))
        {
            _logger.LogWarning("Transición inválida. ErrorCode={ErrorCode}, De={De}, A={A}", "ORD-006", order.Estado, request.Estado);
            throw new ReglaNegocioException("ORD-006",
                $"Una orden en estado '{order.Estado}' no puede pasar a '{request.Estado}'.");
        }

        await _repository.UpdateEstadoAsync(id, request.Estado);
        order.Estado = request.Estado;
        _logger.LogInformation("Estado actualizado. Id={Id}, NuevoEstado={Estado}", id, request.Estado);
        return order;
    }

    public async Task<bool> TieneOrdenesActivasAsync(Guid productoId)
    {
        var ordenes = await _repository.GetAllAsync(null);
        var estadosActivos = new[] { "Pendiente", "Confirmada" };

        return ordenes.Any(o =>
            estadosActivos.Contains(o.Estado) &&
            o.Items.Any(i => i.ProductoId == productoId));
    }

    // Transiciones de estado permitidas
    private static bool EsTransicionValida(string actual, string nuevo)
    {
        var transiciones = new Dictionary<string, string[]>
        {
            ["Pendiente"]  = new[] { "Confirmada", "Cancelada" },
            ["Confirmada"] = new[] { "Enviada", "Cancelada" },
            ["Enviada"]    = new[] { "Entregada" },
            ["Entregada"]  = Array.Empty<string>(),
            ["Cancelada"]  = Array.Empty<string>()
        };

        return transiciones.TryGetValue(actual, out var permitidos) && permitidos.Contains(nuevo);
    }
}