namespace Notifications.API.Exceptions;

public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }
// solo get porque el ErrorCode (NTF-001, NTF-002, etc.) se define cuando se crea la excepción y no tiene sentido cambiarlo después.
    public BusinessRuleException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}