using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;

namespace OrionSoft.AppCore.Application.Exceptions;

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
    /// Gets the original structured error information.
    /// </summary>
    public DictionaryError Error { get; }

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
        Error = error;
        MessageLog = new MessageLog {
            Type = GetType().Name,
            Source = base.Source,
            Message = JsonExtend.ToJsonElement(error),
            Method = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }
    public override string ToString()
         => MessageLog.ToString();
}

internal static class ExceptionHelpers {
    /// <summary>
    /// Collects all inner exception messages into a single newline-separated string.
    /// </summary>
    internal static string? CollectInnerMessages(Exception? innerException) {
        if (innerException is null)
            return null;

        var sb = new System.Text.StringBuilder();
        var current = innerException;
        while (current is not null) {
            if (sb.Length > 0)
                sb.Append("\n\t\t");
            sb.Append(current.Message);
            current = current.InnerException;
        }
        return sb.ToString();
    }
}

/// <summary>
/// Represents a structured error with code, message, and optional additional information.
/// Converted to record for immutability and with-expressions support.
/// </summary>
public sealed record DictionaryError {
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
    /// Gets or initializes the exception details as a string representation.
    /// </summary>
    public string? Exception { get; init; }

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
    /// <param name="exception">Optional exception details as string</param>
    [SetsRequiredMembers]
    public DictionaryError(string code, string message, string? providerMessage = null, string? exception = null) {
        Code = code;
        Message = message;
        Exception = exception;
        if (!string.IsNullOrEmpty(providerMessage))
            ProviderMessage = JsonExtend.ToJsonDocument(providerMessage);
    }
}
