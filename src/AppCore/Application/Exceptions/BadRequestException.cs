using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a bad request error (HTTP 400).
/// Typically thrown when the client sends invalid data or parameters.
/// </summary>
public sealed class BadRequestException : CustomException {
    /// <summary>
    /// Initializes a new instance of the BadRequestException class with the specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    public BadRequestException(string message,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("BAD-REQ-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
