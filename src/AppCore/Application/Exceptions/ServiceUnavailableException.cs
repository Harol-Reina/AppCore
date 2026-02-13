using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

/// <summary>
/// Exception that represents a service unavailable error (HTTP 503).
/// Typically thrown when an external dependency or service is not reachable.
/// </summary>
public sealed class ServiceUnavailableException : CustomException {
    /// <summary>
    /// Gets the name of the unavailable service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Initializes a new instance of the ServiceUnavailableException class.
    /// </summary>
    public ServiceUnavailableException(string serviceName, string message,
                                       [CallerMemberName] string memberName = "",
                                       [CallerFilePath] string sourceFilePath = "",
                                       [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("SVC-UNAVAIL-001", message), memberName, sourceFilePath, sourceLineNumber) {
        ServiceName = serviceName;
    }
}
