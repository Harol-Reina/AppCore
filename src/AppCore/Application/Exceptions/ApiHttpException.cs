using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

internal class ApiHttpException : Exception {
    public MessageLog MessageLog { get; }

    public ApiHttpException(Exception ex,
                            [CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = ""
                            ) : base("The connection to the requested URL cannot be made.") {
        MessageLog = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = $"{ex.Message} :: {memberName}",
            Metodo = memberName,
            Path = sourceFilePath + (ex.ToString().Contains(":line") ? ex.ToString()[ex.ToString().IndexOf(":line")..] : ""),
        };
    }

    public override string ToString()
        => MessageLog.ToString();
}
