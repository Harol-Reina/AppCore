using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class ApiDBException : CustomException {
    public ApiDBException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("API-DB-001", "An error occurred validating an operation in the DB.",
                   null, ExceptionHelpers.CollectInnerMessages(ex)),
               memberName, sourceFilePath, sourceLineNumber) {
    }
}
