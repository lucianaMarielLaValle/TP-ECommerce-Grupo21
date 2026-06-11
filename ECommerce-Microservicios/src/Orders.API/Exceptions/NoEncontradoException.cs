namespace Orders.Api.Exceptions;

/// <summary>
/// Recurso no encontrado. La lanza el Service para los códigos del catálogo que devuelven 404:
/// ORD-001 (orden), ORD-003 (usuario), ORD-004 (producto).
/// El handler asociado siempre responde 404.
/// </summary>
public class NoEncontradoException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
