using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class ApiHttpException : CustomException {
    public ApiHttpException(Exception ex,
                            [CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = "",
                            [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("API-HTTP-001", "The connection to the requested URL cannot be made.",
                   null, ExceptionHelpers.CollectInnerMessages(ex)),
               memberName, sourceFilePath, sourceLineNumber) {
    }
}
