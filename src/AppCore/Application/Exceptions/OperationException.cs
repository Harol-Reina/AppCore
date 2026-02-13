using System.Runtime.CompilerServices;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;

namespace OrionSoft.AppCore.Application.Exceptions;

public sealed class OperationException : Exception {
    readonly MessageLog mensaje;
    public OperationException(string message,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) : base(message) {
        mensaje = new MessageLog {
            Type = base.GetType().Name,
            Source = base.Source,
            Message = JsonExtend.ToJsonElement(message),
            Method = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }
    public override string ToString()
         => mensaje.ToString();
}
