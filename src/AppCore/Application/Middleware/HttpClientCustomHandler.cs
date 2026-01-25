using Microsoft.AspNetCore.Mvc;
using AppCore.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Middleware;

internal class HttpClientCustomHandler(RequestDelegate next) {

    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context) {
        const string TraceIdHeader = "X-Trace-ID";
        // Obtener o generar TraceId con formato consistente
        var traceId = GetOrGenerateTraceId(context, TraceIdHeader);
        

        using (Serilog.Context.LogContext.PushProperty("XTraceID", traceId)) {
            try {
                await _next(context);
            } catch (Exception exceptionObj) {
                await HandleExceptionAsync(context, exceptionObj);
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
        string message = exception switch {
            ApiDBException ex => JsonExtend.Serialize(new ValidationProblemDetails {
                Title = ex.Message
            }),

            ValidationException validationException => JsonExtend.Serialize(new ValidationProblemDetails(validationException.Errors) {
                Title = $"One or more validation errors have occurred."
            }),

            NotFoundException notFoundException => JsonExtend.Serialize(new ErrorResponse(
                notFoundException.Message
            )),

            BadRequestException badRequestException => JsonExtend.Serialize(new ErrorResponse(
                badRequestException.Message
            )),

            AuthenticationException auth => JsonExtend.Serialize(new ProblemDetails {
                Title = "Unauthorized",
                Detail = auth.Message
            }),

            ForbiddenAccessException ex => JsonExtend.Serialize(new ProblemDetails {
                Title = "Forbidden",
                Detail = ex.Message
            }),

            SerializerException serializerException => JsonExtend.Serialize(new ProblemDetails {
                Title = serializerException.Message
            }),

            MappingException mappingException => JsonExtend.Serialize(new MappingErrorResponse(
                mappingException.Message,
                mappingException.Errors
            )),

            ApiHttpException apiHttpException => JsonExtend.Serialize(new ProblemDetails {
                Title = apiHttpException.Message,
                Detail = apiHttpException.MessageLog.Message?.ToString()
            }),

            CustomException customException when customException.MessageLog.Message is DictionaryError error 
                => JsonExtend.Serialize(new CustomErrorResponse(
                    error with { ProviderMessage = null }
                )),

            CustomException customException => JsonExtend.Serialize(new CustomErrorResponse(
                customException.MessageLog.Message!
            )),

            _ => JsonExtend.Serialize(new ProblemDetails {
                Title = "An error occurred while processing your request."
            })
        };

        httpContext.Response.StatusCode = exception switch {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            ForbiddenAccessException => StatusCodes.Status403Forbidden,
            ValidationException or ApiDBException or ApiHttpException or CustomException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(message);
    }
}
