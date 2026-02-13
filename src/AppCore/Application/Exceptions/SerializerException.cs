using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

internal sealed class SerializerException : Exception {
    public MessageLog MessageLog { get; }
    public SerializerException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred while serializing or deserializing an object.") {
        MessageLog = new MessageLog {
            Type = base.GetType().Name,
            Source = base.Source,
            Message = $"{ex.Message.Split("\n").ToArray()[0]}",
            Method = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
            StackTrace = ExceptionHelpers.CollectInnerMessages(ex.InnerException)
        };
    }

    public SerializerException(string message,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred while serializing or deserializing an object.") {

        MessageLog = new MessageLog {
            Type = base.GetType().Name,
            Source = base.Source,
            Message = message,
            Method = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }

    public override string ToString()
        => MessageLog.ToString();
}
