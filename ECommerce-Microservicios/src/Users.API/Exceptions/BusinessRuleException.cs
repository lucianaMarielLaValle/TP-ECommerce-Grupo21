namespace Users.API.Exceptions;

public class BusinessRuleException : Exception
{
    // solo get porque el ErrorCode (USR-001, USR-002, etc.) se define cuando se crea la excepción y no tiene sentido cambiarlo después.
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
