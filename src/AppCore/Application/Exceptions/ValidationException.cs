using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using AppCore.Application.Wrappers;
using FluentValidation.Results;

namespace AppCore.Application.Exceptions;

public class ValidationException : Exception {
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
    private readonly MessageLog _messageLog;
    private string? _innerMessage;

    public ValidationException(IEnumerable<ValidationFailure> failures,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0) {
        Errors = GroupValidationFailures(failures);
        _messageLog = CreateMessageLog("One or more validation errors have occurred.", memberName, sourceFilePath, sourceLineNumber);
    }

    public ValidationException(string propertyName,
                               string errorMessage,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0) {
        Errors = GroupValidationFailures([new ValidationFailure(propertyName, errorMessage)]);
        _messageLog = CreateMessageLog("One or more validation errors have occurred.", memberName, sourceFilePath, sourceLineNumber);
    }

    public ValidationException(Exception ex,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0) {
        Errors = GroupValidationFailures([new ValidationFailure(ex.Source!, ex.Message)]);
        if (ex.InnerException != null) {
            AppendInnerExceptionMessages(ex.InnerException);
        }
        _messageLog = CreateMessageLog("One or more validation errors have occurred.", memberName, sourceFilePath, sourceLineNumber, ex.GetType().Name, ex.Source);
    }

    public ValidationException(ValidationProblemDetails problemDetails,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0) {
        Errors = problemDetails.Errors;
        _messageLog = CreateMessageLog("One or more validation errors have occurred.", memberName, sourceFilePath, sourceLineNumber);
    }

    private static Dictionary<string, string[]> GroupValidationFailures(IEnumerable<ValidationFailure> failures) 
        => failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(group => group.Key, group => group.ToArray());

    private void AppendInnerExceptionMessages(Exception ex) {
        _innerMessage += $"{ex.Message}\n\t\t";
        if (ex.InnerException != null) {
            AppendInnerExceptionMessages(ex.InnerException);
        }
    }

    private MessageLog CreateMessageLog(string validationMessage, string memberName, string sourceFilePath, int sourceLineNumber, string? typeName = null, string? source = null) {
        return new MessageLog {
            Tipo = typeName ?? GetType().Name,
            Source = source ?? base.Source,
            Message = new {
                Validation = validationMessage,
                Errors
            },
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
            StackTrace = _innerMessage
        };
    }

    public override string ToString() 
        => _messageLog.ToString();
}
