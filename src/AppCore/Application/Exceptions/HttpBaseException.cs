using System.Runtime.CompilerServices;
using OrionSoft.AppCore.Application.Wrappers;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class HttpBaseException : CustomException {
    public int StatusCode { get; }

    public HttpBaseException(string message, int statusCode,
                           [CallerMemberName] string memberName = "",
                           [CallerFilePath] string sourceFilePath = "",
                           [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("HTTP-" + statusCode.ToString(), message), memberName, sourceFilePath, sourceLineNumber) {
        StatusCode = statusCode;
    }
}
