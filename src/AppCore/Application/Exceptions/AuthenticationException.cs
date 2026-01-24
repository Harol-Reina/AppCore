using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

public class AuthenticationException : Exception {
    readonly MessageLog mensaje;
    public AuthenticationException(string message,
                                   [CallerMemberName] string memberName = "",
                                   [CallerFilePath] string sourceFilePath = "",
                                   [CallerLineNumber] int sourceLineNumber = 0) : base(message) {
        mensaje = new MessageLog {
            Tipo = base.GetType().Name,
            Source = base.Source,
            Message = message,
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }

    public override string ToString()
        => mensaje.ToString();
}
