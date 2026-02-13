using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

public sealed class OperationException : CustomException {
    public OperationException(string message,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("OPERATION-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
