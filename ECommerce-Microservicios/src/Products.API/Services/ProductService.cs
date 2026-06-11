using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService
{
    private readonly ProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        _logger.LogInformation("Listando productos. Filtros: categoria={Categoria}, nombre={Nombre}", categoria, nombre);
        return await _repository.GetAllAsync(categoria, nombre);
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Producto no encontrado. ErrorCode={ErrorCode}, Id={Id}", "PRD-001", id);
            throw new NoEncontradoException("PRD-001", "Producto no encontrado.");
        }
        return product;
    }

    public async Task<Product> CreateAsync(CrearProductoDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0 || string.IsNullOrWhiteSpace(request.Categoria))
        {
            _logger.LogWarning("Datos de producto inválidos. ErrorCode={ErrorCode}", "PRD-002");
            throw new ValidacionException("PRD-002", "Los datos del producto son inválidos.");
        }

        if (await _repository.ExisteDuplicadoAsync(request.Nombre, request.Categoria))
        {
            _logger.LogWarning("Producto duplicado. ErrorCode={ErrorCode}, Nombre={Nombre}", "PRD-003", request.Nombre);
            throw new ReglaNegocioException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.CreateAsync(product);
        _logger.LogInformation("Producto creado. Id={Id}", product.Id);
        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, ActualizarProductoDTO request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Producto no encontrado para actualizar. ErrorCode={ErrorCode}, Id={Id}", "PRD-001", id);
            throw new NoEncontradoException("PRD-001", "Producto no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0 || request.Stock < 0 || string.IsNullOrWhiteSpace(request.Categoria))
        {
            _logger.LogWarning("Datos de actualización inválidos. ErrorCode={ErrorCode}, Id={Id}", "PRD-002", id);
            throw new ValidacionException("PRD-002", "Los datos del producto son inválidos.");
        }

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        await _repository.UpdateAsync(product);
        _logger.LogInformation("Producto actualizado. Id={Id}", id);
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Producto no encontrado para eliminar. ErrorCode={ErrorCode}, Id={Id}", "PRD-001", id);
            throw new NoEncontradoException("PRD-001", "Producto no encontrado.");
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Producto eliminado. Id={Id}", id);
    }
}