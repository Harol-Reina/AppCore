using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace AppCore.Application.Exceptions;

public sealed class ValidationException : CustomException {
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();

    public ValidationException() : base(new DictionaryError("VAL-000", "Validation failed")) {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message) : base(new DictionaryError("VAL-001", message)) {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("VAL-002", "One or more validation errors have occurred."), memberName, sourceFilePath, sourceLineNumber) {
        Errors = GroupValidationFailures(failures);
    }

    public ValidationException(string propertyName,
                               string errorMessage,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("VAL-003", errorMessage), memberName, sourceFilePath, sourceLineNumber) {
        Errors = new Dictionary<string, string[]> {
            { propertyName, [errorMessage] }
        };
    }

    public ValidationException(Exception ex,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("VAL-004", "One or more validation errors have occurred."), memberName, sourceFilePath, sourceLineNumber) {
        Errors = GroupValidationFailures([new ValidationFailure(ex.Source ?? "Unknown", ex.Message)]);
    }

    public ValidationException(ValidationProblemDetails problemDetails,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("VAL-005", "One or more validation errors have occurred."), memberName, sourceFilePath, sourceLineNumber) {
        Errors = problemDetails.Errors;
    }

    private static Dictionary<string, string[]> GroupValidationFailures(IEnumerable<ValidationFailure> failures)
        => failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(group => group.Key, group => group.ToArray());
}
