using System.Collections.Frozen;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Extensions;
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
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        try {
            return await next().ConfigureAwait(false);
        } catch (Exception ex) {
            // AOT-compatible exception handling using pattern matching instead of reflection
            if (IsKnownException(ex)) {
                _logger.LogError("AppCore Request: {Request}", request);
                throw;
            } else {
                var innerMessage = ex.InnerException != null
                    ? ExceptionHelpers.CollectInnerMessages(ex.InnerException)
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
                _logger.LogError("AppCore Request: {Request} \n{Message}", request, message);
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
        typeof(BadRequestException),
        typeof(NotFoundException),
        typeof(ForbiddenAccessException),
        typeof(AuthenticationException),
        typeof(ValidationException),
        typeof(ConflictException),
        typeof(UnprocessableEntityException),
        typeof(ServiceUnavailableException),
        typeof(GatewayTimeoutException),
        typeof(OperationException),
        typeof(MappingException),
        typeof(SerializerException),
    }.ToFrozenSet();

    /// <summary>
    /// AOT-compatible method to check if an exception is a known application exception.
    /// Uses declarative FrozenSet lookup instead of switch expression to minimize cyclomatic complexity.
    /// </summary>
    private static bool IsKnownException(Exception exception) =>
        _knownExceptionTypes.Contains(exception.GetType());
}
