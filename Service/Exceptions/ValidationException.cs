using System.Net;

namespace Service.Exceptions;

public class ValidationException : Exception
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;
    public string Code { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public override string Message => ErrorMessage;
    
    public ValidationException(string message, string code) : base(message)
    {
        Code = code;
    }
    
    public ValidationException(string message, string code, HttpStatusCode statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public ValidationException(): base("Validation failed")
    {
        Code = "400";
        StatusCode = HttpStatusCode.BadRequest;
    }
}