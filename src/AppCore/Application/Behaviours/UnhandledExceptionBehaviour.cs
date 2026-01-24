using Microsoft.Extensions.Logging;
using AppCore.Application.Exceptions;
using AppCore.Application.Wrappers;
using MediatR;

namespace AppCore.Application.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger = logger;
    private readonly IList<Type> exceptionHandlers = [
            typeof(ApiDBException) ,
            typeof(ValidationException),
            typeof(ApiHttpException),
            typeof(OperationException),
            typeof(NotFoundException),
            typeof(BadRequestException),
            typeof(ForbiddenAccessException),
            typeof(AuthenticationException),
            typeof(MappingException),
            typeof(SerializerException),
            typeof(CustomException),
        ];
    private string InnerMessage = string.Empty;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        try {
            return await next();
        } catch (Exception ex) {
            Type type = ex.GetType();
            if (exceptionHandlers.Contains(type)) {
                _logger.LogError("CleanArchitecture Request: {Request}", request);
                throw;
            } else {
                if (ex.InnerException != null)
                    ShowInnerExceptionMessages(ex.InnerException);
                InnerMessage = InnerMessage.Replace("\r\n", " ")
                                           .Replace("\n", " ")
                                           .Replace("\r", " ");
                var menssage = new MessageLog {
                    Tipo = $"{ex.GetType()}",
                    Source = ex.Source,
                    Message = $"{ex.Message.Split('\n').ToArray()[0]}",
                    Metodo = "",
                    Path = ex.StackTrace?
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => line.Contains(".Infrastructure"))
                            .Select(line => line.Trim())
                            .FirstOrDefault() ?? string.Empty,
                    StackTrace = InnerMessage
                };
                _logger.LogError("CleanArchitecture Request: {Request} \n{Message}", request, menssage);
                throw;
            }
        }
    }

    private void ShowInnerExceptionMessages(Exception ex) {
        InnerMessage += ex.Message;
        if (ex.InnerException != null) {
            InnerMessage += "\n\t\t";
            ShowInnerExceptionMessages(ex.InnerException);
        }
    }
}
