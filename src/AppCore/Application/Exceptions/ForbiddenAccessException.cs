using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

public sealed class ForbiddenAccessException : CustomException {
    public ForbiddenAccessException(string message,
                                   [CallerMemberName] string memberName = "",
                                   [CallerFilePath] string sourceFilePath = "",
                                   [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("FORBID-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
