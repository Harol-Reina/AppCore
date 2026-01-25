using System.Runtime.CompilerServices;

namespace AppCore.Application.Exceptions;

public class NotFoundException : HttpBaseException {
    public NotFoundException(string message = "The requested resource could not be found.",
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0) : base(message, 404, memberName, sourceFilePath, sourceLineNumber) {
    }
}
