using System.Runtime.CompilerServices;
using AppCore.Application.Wrappers;
using AutoMapper;

namespace AppCore.Application.Exceptions;

public class MappingException : Exception {
    public IDictionary<string, string> Errors { get; init; } = new Dictionary<string, string>();
    readonly MessageLog mensaje;
    public MappingException(AutoMapperMappingException ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0) : base("Missing type map configuration or unsupported mapping.") {
        string message = "Unknown mapping error.";
        if (ex.Types.HasValue && ex.Types.Value.SourceType != null && ex.Types.Value.DestinationType != null) {
            Errors.Add("SourceType", ex.Types.Value.SourceType.Name);
            Errors.Add("DestinationType", ex.Types.Value.DestinationType.Name);
            message = "Missing type unsupported mapping.";
        }
        if (ex.TypeMap != null) {
            Errors.Add("SourceType", ex.TypeMap.SourceType.Name);
            Errors.Add("DestinationType", ex.TypeMap.DestinationType.Name);
            message = "Missing type map configuration.";
        }
        
        mensaje = new MessageLog {
            Tipo = ex.GetType().Name,
            Source = ex.Source,
            Message = new {
                Validation = message,
                Errors
            },
            Metodo = memberName,
            Path = $"{sourceFilePath} Line: {sourceLineNumber}",
        };
    }

    public override string ToString()
         => mensaje.ToString();
}
