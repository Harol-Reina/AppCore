using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

/// <summary>
/// Base class for custom application exceptions that provides enhanced error tracking and logging capabilities.
/// Captures caller information automatically for better diagnostics.
/// </summary>
public class CustomException : Exception {
    /// <summary>
    /// Gets the detailed error message log containing caller information and context.
    /// </summary>
    /// <value>A MessageLog instance with error details and caller context.</value>
    public MessageLog MessageLog { get; }
    
    /// <summary>
    /// Initializes a new instance of the CustomException class with a dictionary error.
    /// </summary>
    /// <param name="error">The structured error information.</param>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    public CustomException(DictionaryError error,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) : base(error.Message) {
        MessageLog = new MessageLog {
            Tipo = GetType().Name,
            Source = base.Source,
            Message = error,
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }
    public override string ToString()
         => MessageLog.ToString();
}

/// <summary>
/// Represents a structured error with code, message, and optional additional information.
/// Converted to record for immutability and with-expressions support.
/// </summary>
public record DictionaryError {
    /// <summary>
    /// Gets or initializes the error code.
    /// </summary>
    public required string Code { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or initializes the error message.
    /// </summary>
    public required string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or initializes additional provider message information as JSON.
    /// </summary>
    public JsonDocument? ProviderMessage { get; init; }
    
    /// <summary>
    /// Gets or initializes the exception details.
    /// Changed from dynamic to object for AOT compatibility.
    /// </summary>
    public object? Exception { get; init; }

    /// <summary>
    /// Initializes a new instance of the DictionaryError record.
    /// </summary>
    public DictionaryError() { }

    /// <summary>
    /// Initializes a new instance of the DictionaryError record with specified values.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    /// <param name="providerMessage">Optional provider message</param>
    /// <param name="exception">Optional exception details (changed from dynamic to object for AOT)</param>
    [SetsRequiredMembers]
    public DictionaryError(string code, string message, string? providerMessage = null, object? exception = null) {
        Code = code;
        Message = message;
        Exception = exception;
        if (!string.IsNullOrEmpty(providerMessage))
            ProviderMessage = JsonExtend.ToJsonDocument(providerMessage);
    }
}
