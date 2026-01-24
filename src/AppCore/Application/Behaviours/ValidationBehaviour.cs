using AppCore.Application.Exceptions;
using MediatR;

namespace AppCore.Application.Behaviours;
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<FluentValidation.IValidator<TRequest>> validators) 
: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> {

    private readonly IEnumerable<FluentValidation.IValidator<TRequest>> validators = validators;

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
