namespace Products.API.Exceptions;

public class ReglaNegocioException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}