using Microsoft.AspNetCore.Mvc;
using AppCore.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using AppCore.Application.Extensions;

namespace AppCore.Application.Middleware;

public class HttpClientCustomHandler(RequestDelegate next) {

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
        string message;
        switch (exception) {
            case ApiDBException _:
                message = JsonExtend.Serialize(new ValidationProblemDetails {
                    Title = exception.Message
                });
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case ValidationException validationException:
                message = JsonExtend.Serialize(new ValidationProblemDetails(validationException.Errors) {
                    Title = $"One or more validation errors have occurred."
                });
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case NotFoundException notFoundException:
                message = JsonExtend.Serialize(new {
                    message = notFoundException.Message,
                });
                httpContext.Response.StatusCode = notFoundException.StatusCode;
                break;

            case BadRequestException badRequestException:
                message = JsonExtend.Serialize(new {
                    message = badRequestException.Message,
                });
                httpContext.Response.StatusCode = badRequestException.StatusCode;
                break;

            case AuthenticationException auth:
                message = JsonExtend.Serialize(new ProblemDetails {
                    Title = "Unauthorized",
                    Detail = auth.Message
                });
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                break;

            case ForbiddenAccessException _:
                message = JsonExtend.Serialize(new ProblemDetails {
                    Title = "Forbidden",
                    Detail = exception.Message
                });
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                break;

            case SerializerException serializerException:
                message = JsonExtend.Serialize(new ProblemDetails {
                    Title = serializerException.Message
                });
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;

            case MappingException mappingException:
                message = JsonExtend.Serialize(new {
                    Title = mappingException.Message,
                    mappingException.Errors
                });
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;

            case ApiHttpException apiHttpException:
                message = JsonExtend.Serialize(new ProblemDetails {
                    Title = apiHttpException.Message,
                    Detail = apiHttpException.MessageLog.Message
                });
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case CustomException customException:
                if (customException.MessageLog.Message is DictionaryError error) {
                    error.ProviderMessage = null;
                    message = JsonExtend.Serialize(new {
                        error
                    });
                } else {
                    message = JsonExtend.Serialize(new {
                        Error = customException.MessageLog.Message
                    });
                }
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                message = JsonExtend.Serialize(new ProblemDetails {
                    Title = "An error occurred while processing your request."
                });
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(message);
    }
}
