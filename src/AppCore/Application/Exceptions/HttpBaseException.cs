namespace AppCore.Application.Exceptions;

public class HttpBaseException(string message, int statusCode) : Exception(message) {
    public int StatusCode { get; } = statusCode;
}