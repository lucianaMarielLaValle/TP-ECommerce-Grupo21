namespace Orders.Api.Exceptions;

/// <summary>
/// Violación de una regla de negocio. En el catálogo de Orders dos códigos pasan por acá
/// con HTTP distinto, por eso la excepción transporta el StatusCode:
///   ORD-005 → 422 (stock insuficiente)
///   ORD-006 → 409 (estado no modificable)
/// El handler responde con el StatusCode que traiga la excepción (default 409).
/// </summary>
/// <example>
/// throw new ReglaNegocioException("ORD-005", "Stock insuficiente para '...'. Disponible: 2, solicitado: 5.", 422);
/// throw new ReglaNegocioException("ORD-006", "Una orden en estado 'Entregada' no puede volver a 'Pendiente'.", 409);
/// </example>
public class ReglaNegocioException(string errorCode, string message, int statusCode = 409) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}
