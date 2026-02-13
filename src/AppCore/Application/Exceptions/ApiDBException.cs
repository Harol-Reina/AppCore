using System.Runtime.CompilerServices;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class ApiDBException : Exception {
    readonly MessageLog mensaje;
    public ApiDBException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
                          : base("An error occurred validating an operation in the DB.") {
        mensaje = new MessageLog {
            Type = base.GetType().Name,
            Source = base.Source,
            Message = JsonExtend.ToJsonElement($"{ex.Message.Split("\n").ToArray()[0]}"),
            Method = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
            StackTrace = ExceptionHelpers.CollectInnerMessages(ex.InnerException)
        };
    }

    public override string ToString()
        => mensaje.ToString();
}
