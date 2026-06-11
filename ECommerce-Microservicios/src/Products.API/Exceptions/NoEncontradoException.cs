namespace Products.API.Exceptions;

public class NoEncontradoException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}