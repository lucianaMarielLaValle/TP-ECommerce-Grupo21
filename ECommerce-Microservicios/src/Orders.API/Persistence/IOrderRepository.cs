using Orders.Api.Models;

namespace Orders.Api.Persistence;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId);
    Task<Order?> GetByIdAsync(Guid id);
    Task CreateAsync(Order order);
    Task UpdateEstadoAsync(Guid id, string estado);
}
