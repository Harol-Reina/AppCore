using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

internal sealed class SerializerException : Exception {
    public MessageLog MessageLog { get; }
    private string? InnerMessage;
    public SerializerException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred while serializing or deserializing an object.") {
        if (ex.InnerException != null)
            ShowInnerExceptionMessages(ex.InnerException);
        MessageLog = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = $"{ex.Message.Split("\n").ToArray()[0]}",
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
            StackTrace = InnerMessage
        };
    }

    public SerializerException(string message,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred while serializing or deserializing an object.") {

        MessageLog = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = message,
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
        => MessageLog.ToString();
}
