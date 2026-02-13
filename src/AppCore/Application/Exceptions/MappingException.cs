using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception thrown when object mapping operations fail.
/// This is an AOT-compatible version that doesn't depend on AutoMapper.
/// </summary>
internal sealed class MappingException : CustomException {
    /// <summary>
    /// Gets the mapping error details.
    /// </summary>
    public IDictionary<string, string> Errors { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Initializes a new instance of the MappingException class.
    /// </summary>
    public MappingException(
        string message,
        Exception? innerException = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("MAPPING-001", message, null, ExceptionHelpers.CollectInnerMessages(innerException)),
               memberName, sourceFilePath, sourceLineNumber) {
        if (innerException != null) {
            Errors.Add("InnerExceptionType", innerException.GetType().Name);
            Errors.Add("InnerExceptionMessage", innerException.Message);
        }
    }
}
