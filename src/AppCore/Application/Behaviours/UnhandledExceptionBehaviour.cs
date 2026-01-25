using Microsoft.Extensions.Logging;
using AppCore.Application.Exceptions;
using AppCore.Application.Wrappers;
using MediatR;

namespace AppCore.Application.Behaviours;

/// <summary>
/// Pipeline behavior for handling unhandled exceptions in MediatR requests.
/// AOT-compatible implementation using pattern matching instead of reflection.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response being returned</typeparam>
internal class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger = logger;
    private string InnerMessage = string.Empty;

    /// <summary>
    /// Handles the request and manages any unhandled exceptions using AOT-compatible pattern matching.
    /// </summary>
    /// <param name="request">The request being processed</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        try {
            return await next();
        } catch (Exception ex) {
            // AOT-compatible exception handling using pattern matching instead of reflection
            if (IsKnownException(ex)) {
                _logger.LogError("CleanArchitecture Request: {Request}", request);
                throw;
            } else {
                if (ex.InnerException != null)
                    ShowInnerExceptionMessages(ex.InnerException);
                InnerMessage = InnerMessage.Replace("\r\n", " ")
                                           .Replace("\n", " ")
                                           .Replace("\r", " ");
                var menssage = new MessageLog {
                    Tipo = ex.GetType().Name, // Use Name instead of ToString() for better AOT compatibility
                    Source = ex.Source,
                    Message = ex.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty,
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

    /// <summary>
    /// AOT-compatible method to check if an exception is a known application exception.
    /// Uses pattern matching instead of reflection-based type checking.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception is a known application exception</returns>
    private static bool IsKnownException(Exception exception) => exception switch {
        ApiDBException => true,
        ValidationException => true,
        ApiHttpException => true,
        OperationException => true,
        NotFoundException => true,
        BadRequestException => true,
        ForbiddenAccessException => true,
        AuthenticationException => true,
        MappingException => true,
        SerializerException => true,
        CustomException => true,
        _ => false
    };

    /// <summary>
    /// Recursively collects inner exception messages for logging.
    /// </summary>
    /// <param name="ex">The exception to process</param>
    private void ShowInnerExceptionMessages(Exception ex) {
        InnerMessage += ex.Message;
        if (ex.InnerException != null) {
            InnerMessage += "\n\t\t";
            ShowInnerExceptionMessages(ex.InnerException);
        }
    }
}
