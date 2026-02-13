using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a conflict error (HTTP 409).
/// Typically thrown when a resource already exists or a concurrent modification conflict occurs.
/// </summary>
public sealed class ConflictException : CustomException {
    /// <summary>
    /// Initializes a new instance of the ConflictException class with the specified error message.
    /// </summary>
    public ConflictException(string message,
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("CONFLICT-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }

    /// <summary>
    /// Initializes a new instance of the ConflictException class for a specific entity and key.
    /// </summary>
    public ConflictException(string entityName, object key,
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("CONFLICT-001", $"{entityName} with key '{key}' already exists."),
               memberName, sourceFilePath, sourceLineNumber) {
    }
}
