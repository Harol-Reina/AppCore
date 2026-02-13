using System.Runtime.CompilerServices;
using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

/// <summary>
/// Exception thrown when object mapping operations fail.
/// This is an AOT-compatible version that doesn't depend on AutoMapper.
/// </summary>
internal sealed class MappingException : Exception {
    /// <summary>
    /// Gets the mapping error details.
    /// </summary>
    public IDictionary<string, string> Errors { get; init; } = new Dictionary<string, string>();

    private readonly MessageLog _message;

    /// <summary>
    /// Initializes a new instance of the MappingException class.
    /// </summary>
    /// <param name="message">The error message describing the mapping failure</param>
    /// <param name="innerException">The inner exception that caused the mapping failure</param>
    /// <param name="memberName">The calling member name (automatically captured)</param>
    /// <param name="sourceFilePath">The source file path (automatically captured)</param>
    /// <param name="sourceLineNumber">The source line number (automatically captured)</param>
    public MappingException(
        string message,
        Exception? innerException = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) : base(message, innerException) {
        if (innerException != null) {
            Errors.Add("InnerExceptionType", innerException.GetType().Name);
            Errors.Add("InnerExceptionMessage", innerException.Message);
        }

        _message = new MessageLog {
            Type = nameof(MappingException),
            Source = sourceFilePath,
            Message = JsonExtend.ToJsonElement(message),
            Method = memberName,
            Path = $"{sourceFilePath}:{sourceLineNumber}",
            StackTrace = StackTrace ?? string.Empty
        };
    }

    /// <summary>
    /// Gets the structured message log for this mapping exception.
    /// </summary>
    /// <returns>The message log containing detailed error information</returns>
    public MessageLog GetMessageLog() => _message;
}
