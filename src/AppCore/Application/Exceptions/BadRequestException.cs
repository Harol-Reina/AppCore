namespace AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a bad request error (HTTP 400).
/// Typically thrown when the client sends invalid data or parameters.
/// </summary>
public sealed class BadRequestException : CustomException {
    /// <summary>
    /// Initializes a new instance of the BadRequestException class with the specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public BadRequestException(string message) : base(new DictionaryError("BAD-REQ-001", message)) {
    }
}
