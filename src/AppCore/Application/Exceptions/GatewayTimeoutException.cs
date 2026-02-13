using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a gateway timeout error (HTTP 504).
/// Typically thrown when an upstream service does not respond in time.
/// </summary>
public sealed class GatewayTimeoutException : CustomException {
    /// <summary>
    /// Initializes a new instance of the GatewayTimeoutException class with the specified error message.
    /// </summary>
    public GatewayTimeoutException(string message,
                                   [CallerMemberName] string memberName = "",
                                   [CallerFilePath] string sourceFilePath = "",
                                   [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("TIMEOUT-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
