namespace Cart.API.Exceptions;

/// <summary>
/// Datos inválidos. 
/// </summary>
public class ValidacionException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
