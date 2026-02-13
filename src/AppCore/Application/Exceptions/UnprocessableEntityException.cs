using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents an unprocessable entity error (HTTP 422).
/// Typically thrown when the request is well-formed but semantically invalid.
/// </summary>
public sealed class UnprocessableEntityException : CustomException {
    /// <summary>
    /// Initializes a new instance of the UnprocessableEntityException class with the specified error message.
    /// </summary>
    public UnprocessableEntityException(string message,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("UNPROCESSABLE-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
