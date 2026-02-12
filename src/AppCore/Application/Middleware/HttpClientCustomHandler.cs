using AppCore.Application.Exceptions;
using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppCore.Application.Middleware;

public sealed class HttpClientCustomHandler(RequestDelegate next) {

    private readonly RequestDelegate _next = next;

    private record ExceptionMapping(int StatusCode, Func<Exception, string> MessageFactory);

    private static readonly Dictionary<Type, ExceptionMapping> ExceptionMappings = new() {
        [typeof(ApiDBException)] = new(StatusCodes.Status400BadRequest, ex =>
            JsonExtend.Serialize(new ValidationProblemDetails { Title = ex.Message })),

        [typeof(ValidationException)] = new(StatusCodes.Status400BadRequest, ex =>
            JsonExtend.Serialize(new ValidationProblemDetails(((ValidationException)ex).Errors) {
                Title = "One or more validation errors have occurred."
            })),

        [typeof(NotFoundException)] = new(StatusCodes.Status404NotFound, ex =>
            JsonExtend.Serialize(new ErrorResponse(ex.Message))),

        [typeof(BadRequestException)] = new(StatusCodes.Status400BadRequest, ex =>
            JsonExtend.Serialize(new ErrorResponse(ex.Message))),

        [typeof(AuthenticationException)] = new(StatusCodes.Status401Unauthorized, ex =>
            JsonExtend.Serialize(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message })),

        [typeof(ForbiddenAccessException)] = new(StatusCodes.Status403Forbidden, ex =>
            JsonExtend.Serialize(new ProblemDetails { Title = "Forbidden", Detail = ex.Message })),

        [typeof(SerializerException)] = new(StatusCodes.Status500InternalServerError, ex =>
            JsonExtend.Serialize(new ProblemDetails { Title = ex.Message })),

        [typeof(MappingException)] = new(StatusCodes.Status500InternalServerError, ex => {
            var mappingEx = (MappingException)ex;
            return JsonExtend.Serialize(new MappingErrorResponse(mappingEx.Message, mappingEx.Errors));
        }),

        [typeof(ApiHttpException)] = new(StatusCodes.Status400BadRequest, ex => {
            var apiHttpEx = (ApiHttpException)ex;
            return JsonExtend.Serialize(new ProblemDetails {
                Title = apiHttpEx.Message,
                Detail = apiHttpEx.MessageLog.Message?.ToString()
            });
        }),

        [typeof(CustomException)] = new(StatusCodes.Status400BadRequest, ex => {
            var customEx = (CustomException)ex;
            return customEx.MessageLog.Message is DictionaryError error
                ? JsonExtend.Serialize(new CustomErrorResponse(error with { ProviderMessage = null }))
                : JsonExtend.Serialize(new CustomErrorResponse(customEx.MessageLog.Message!));
        })
    };

    private static readonly ExceptionMapping DefaultMapping = new(
        StatusCodes.Status500InternalServerError,
        _ => JsonExtend.Serialize(new ProblemDetails {
            Title = "An error occurred while processing your request."
        })
    );

    public async Task Invoke(HttpContext context) {
        const string TraceIdHeader = "X-Trace-ID";
        var traceId = GetOrGenerateTraceId(context, TraceIdHeader);

        using (Serilog.Context.LogContext.PushProperty("XTraceID", traceId)) {
            try {
                await _next(context).ConfigureAwait(false);
            } catch (Exception exceptionObj) {
                await HandleExceptionAsync(context, exceptionObj).ConfigureAwait(false);
            }
        }
    }

    private static string GetOrGenerateTraceId(HttpContext context, string headerName) {
        var traceId = context.Request.Headers[headerName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = Guid.NewGuid().ToString("N");
        return traceId;
    }

    public static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception) {
        var mapping = ExceptionMappings.GetValueOrDefault(exception.GetType()) ?? DefaultMapping;

        httpContext.Response.StatusCode = mapping.StatusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(mapping.MessageFactory(exception)).ConfigureAwait(false);
    }
}
