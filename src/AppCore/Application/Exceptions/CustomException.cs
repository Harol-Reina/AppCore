using System.Runtime.CompilerServices;
using System.Text.Json;
using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Exceptions;

public class CustomException : Exception {
    public MessageLog MessageLog { get; }
    public CustomException(DictionaryError error,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) : base("An error occurred while processing your request.") {
        MessageLog = new MessageLog {
            Tipo = GetType().Name,
            Source = base.Source,
            Message = error,
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }
    public override string ToString()
         => MessageLog.ToString();
}

public class DictionaryError {
    public string Code { get; set; }
    public string Message { get; set; }
    public JsonDocument? ProviderMessage { get; set; }
    public dynamic? Exception { get; set; }

    public DictionaryError(string code, string message, string? providerMessage = null, dynamic? exception = null) {
        Code = code;
        Message = message;
        Exception = exception;
        if (!string.IsNullOrEmpty(providerMessage))
            ProviderMessage = JsonExtend.ToJsonDocument(providerMessage);
    }
       
}