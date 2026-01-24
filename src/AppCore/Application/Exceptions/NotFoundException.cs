using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

public class NotFoundException : HttpBaseException {
    public MessageLog MessageLog { get; }
    public NotFoundException(string message = "The requested resource could not be found.",
                             [CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0) : base(message, 404) {
         MessageLog = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = message,
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }

    public override string ToString()
        => MessageLog.ToString();
}
