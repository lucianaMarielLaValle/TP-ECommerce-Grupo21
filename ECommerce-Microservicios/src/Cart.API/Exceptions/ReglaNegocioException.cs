namespace Cart.API.Exceptions;

/// <summary>
/// Violación de una regla de negocio.
/// </summary>

public class ReglaNegocioException(string errorCode, string message, int statusCode = 409) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}
