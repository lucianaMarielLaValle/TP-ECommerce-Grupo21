namespace Orders.API.Exceptions;

/// <summary>
/// Datos inválidos. La lanza el Service para ORD-002 (400) cuando una validación
/// no cubierta por Data Annotations falla (por ejemplo, reglas de negocio sobre el body).
/// El handler asociado siempre responde 400.
/// </summary>
public class ValidacionException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
