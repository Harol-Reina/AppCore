using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a resource not found error (HTTP 404).
/// Typically thrown when a requested resource cannot be located.
/// </summary>
public sealed class NotFoundException : CustomException {
    /// <summary>
    /// Initializes a new instance of the NotFoundException class.
    /// </summary>
    /// <param name="message">The error message describing what was not found.</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    public NotFoundException(string message = "The requested resource could not be found.",
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("NOT-FOUND-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }

    /// <summary>
    /// Initializes a new instance of the NotFoundException class for a specific entity and key.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="key">The key value that was searched for.</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    public NotFoundException(string entityName, object key,
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("NOT-FOUND-002", $"{entityName} with key '{key}' was not found."),
               memberName, sourceFilePath, sourceLineNumber) {
    }
}
