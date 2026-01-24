namespace AppCore.Application.Exceptions;

public class BadRequestException : HttpBaseException {
    public BadRequestException(string message) : base(message, 400) {
    }
}
