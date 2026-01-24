using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

public class ApiDBException : Exception {
    readonly MessageLog mensaje;
    private string? InnerMessage;
    public ApiDBException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred validating an operation in the DB.") {
        if (ex.InnerException != null)
            ShowInnerExceptionMessages(ex.InnerException);
        mensaje = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = $"{ex.Message.Split("\n").ToArray()[0]}",
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
            StackTrace = InnerMessage
        };
    }

    private void ShowInnerExceptionMessages(Exception ex) {
        InnerMessage += ex.Message;
        if (ex.InnerException != null) {
            InnerMessage += "\n\t\t";
            ShowInnerExceptionMessages(ex.InnerException);
        }
    }

    public override string ToString()
        => mensaje.ToString();
}
