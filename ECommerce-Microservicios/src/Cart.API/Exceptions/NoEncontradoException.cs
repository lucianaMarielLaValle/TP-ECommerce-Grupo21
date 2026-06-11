namespace Cart.API.Exceptions;

/// <summary>
/// Recurso no encontrado. 
/// </summary>
public class NoEncontradoException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
