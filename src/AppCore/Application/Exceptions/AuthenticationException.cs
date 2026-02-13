using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

public sealed class AuthenticationException : CustomException {
    public AuthenticationException(string message,
                                   [CallerMemberName] string memberName = "",
                                   [CallerFilePath] string sourceFilePath = "",
                                   [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("AUTH-001", message), memberName, sourceFilePath, sourceLineNumber) {
    }
}
