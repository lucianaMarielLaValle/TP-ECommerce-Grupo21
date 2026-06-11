namespace Products.API.Exceptions;

public class ValidacionException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}