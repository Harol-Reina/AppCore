using AppCore.Application.Exceptions;
using MediatR;

namespace AppCore.Application.Behaviours;

/// <summary>
/// Pipeline behavior for validating MediatR requests using FluentValidation.
/// This behavior runs before the request handler and validates the request using registered validators.
/// </summary>
/// <typeparam name="TRequest">The type of request being validated</typeparam>
/// <typeparam name="TResponse">The type of response being returned</typeparam>
internal class ValidationBehaviour<TRequest, TResponse>(IEnumerable<FluentValidation.IValidator<TRequest>> validators) 
: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {

    private readonly IEnumerable<FluentValidation.IValidator<TRequest>> validators = validators;

    /// <summary>
    /// Handles the request validation pipeline.
    /// Validates the request using all registered validators and throws ValidationException if validation fails.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next handler if validation passes</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle (TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        if (validators.Any()) {
            var context = new FluentValidation.ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        return await next();
    }
}
