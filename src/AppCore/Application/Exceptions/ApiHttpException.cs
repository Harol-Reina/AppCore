using System.Runtime.CompilerServices;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class ApiHttpException : Exception {
    public MessageLog MessageLog { get; }

    public ApiHttpException(Exception ex,
                            [CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = ""
                            ) : base("The connection to the requested URL cannot be made.") {
        MessageLog = new MessageLog {
            Type = base.GetType().Name,
            Source = base.Source,
            Message = JsonExtend.ToJsonElement($"{ex.Message} :: {memberName}"),
            Method = memberName,
            Path = sourceFilePath + (ex.ToString().Contains(":line") ? ex.ToString()[ex.ToString().IndexOf(":line")..] : ""),
        };
    }

    public override string ToString()
        => MessageLog.ToString();
}
