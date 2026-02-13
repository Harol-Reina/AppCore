using System.Collections.Frozen;
using System.Text;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrionSoft.AppCore.Application.Behaviours;

/// <summary>
/// Pipeline behavior for handling unhandled exceptions in MediatR requests.
/// AOT-compatible implementation using pattern matching instead of reflection.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response being returned</typeparam>
internal sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handles the request and manages any unhandled exceptions using AOT-compatible pattern matching.
    /// </summary>
    /// <param name="request">The request being processed</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        try {
            return await next().ConfigureAwait(false);
        } catch (Exception ex) {
            // AOT-compatible exception handling using pattern matching instead of reflection
            if (IsKnownException(ex)) {
                _logger.LogError("CleanArchitecture Request: {Request}", request);
                throw;
            } else {
                var innerMessage = ex.InnerException != null
                    ? CollectInnerMessages(ex.InnerException)
                    : string.Empty;
                var message = new MessageLog {
                    Type = ex.GetType().Name,
                    Source = ex.Source,
                    Message = JsonExtend.ToJsonElement(ex.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty),
                    Method = "",
                    Path = ex.StackTrace?
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => line.Contains(".Infrastructure"))
                            .Select(line => line.Trim())
                            .FirstOrDefault() ?? string.Empty,
                    StackTrace = innerMessage
                };
                _logger.LogError("CleanArchitecture Request: {Request} \n{Message}", request, message);
                throw;
            }
        }
    }

    /// <summary>
    /// Declarative set of known application exception types for AOT-compatible lookup.
    /// </summary>
    private static readonly FrozenSet<Type> _knownExceptionTypes = new Type[] {
        typeof(ApiDBException),
        typeof(ApiHttpException),
        typeof(CustomException),
        typeof(HttpBaseException),
        typeof(BadRequestException),
        typeof(NotFoundException),
        typeof(ForbiddenAccessException),
        typeof(AuthenticationException),
        typeof(ValidationException),
        typeof(OperationException),
        typeof(MappingException),
        typeof(SerializerException),
    }.ToFrozenSet();

    /// <summary>
    /// AOT-compatible method to check if an exception is a known application exception.
    /// Uses declarative FrozenSet lookup instead of switch expression to minimize cyclomatic complexity.
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception is a known application exception</returns>
    private static bool IsKnownException(Exception exception) =>
        _knownExceptionTypes.Contains(exception.GetType());

    /// <summary>
    /// Iteratively collects inner exception messages for logging.
    /// </summary>
    /// <param name="ex">The first inner exception to process</param>
    /// <returns>A single-line string with all inner exception messages</returns>
    private static string CollectInnerMessages(Exception ex) {
        var sb = new StringBuilder();
        var current = ex;
        while (current != null) {
            if (sb.Length > 0) sb.Append("\n\t\t");
            sb.Append(current.Message);
            current = current.InnerException;
        }
        return sb.ToString().Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
    }
}
