namespace DotnetAuthentication.Helper;

public class Exceptions : Exception
{
    public int StatusCode { get; }

    public Exceptions(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    // Specific exception types
    public static Exceptions BadRequest(string message) => new Exceptions(message, 400);
    public static Exceptions Unauthorized(string message) => new Exceptions(message, 401);
    public static Exceptions Forbidden(string message) => new Exceptions(message, 403);
    public static Exceptions NotFound(string message) => new Exceptions(message, 404);
}