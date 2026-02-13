using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Exceptions;

internal sealed class SerializerException : CustomException {
    public SerializerException(Exception ex,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("SERIALIZER-001", "An error occurred while serializing or deserializing an object.",
                   null, ExceptionHelpers.CollectInnerMessages(ex)),
               memberName, sourceFilePath, sourceLineNumber) {
    }

    public SerializerException(string message,
                          [CallerMemberName] string memberName = "",
                          [CallerFilePath] string sourceFilePath = "",
                          [CallerLineNumber] int sourceLineNumber = 0)
        : base(new DictionaryError("SERIALIZER-002", "An error occurred while serializing or deserializing an object.", message),
               memberName, sourceFilePath, sourceLineNumber) {
    }
}
