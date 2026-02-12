using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using AppCore.Application.Exceptions;
using AppCore.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AppCore.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService {
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private static string? GetOptionalHeaderValue(HttpContext httpContext, string headerName) {
        if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValues)) {
            return headerValues.FirstOrDefault()?.Trim();
        }
        return null; // Devuelve null si no existe el encabezado
    }

    public string GetXtraceId([CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) {

        var httpContext = _httpContextAccessor.HttpContext ?? throw new AuthenticationException("HttpContext not found.", memberName, sourceFilePath, sourceLineNumber);
        var traceId = GetOptionalHeaderValue(httpContext, "X-Trace-ID");
        if (string.IsNullOrEmpty(traceId))
            return Guid.NewGuid().ToString();
        return traceId;
    }

    // Método para obtener un encabezado obligatorio
    private static string GetRequiredHeaderValue(HttpContext httpContext, string headerName, string displayName, [CallerMemberName] string memberName = "",
                                                 [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
        var value = GetOptionalHeaderValue(httpContext, headerName);
        if (string.IsNullOrEmpty(value)) {
            // para guardarlo en la base de datos con createdby y updatedby
            throw new AuthenticationException($"{displayName} not found in headers.", memberName, sourceFilePath, sourceLineNumber);
        }
        return value;
    }

    // Método para obtener un claim obligatorio
    private static string GetRequiredClaimValue(JwtSecurityToken token, string claimType, [CallerMemberName] string memberName = "",
                                                [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
        var claim = token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrEmpty(claim)) {
            throw new CustomException(
                new DictionaryError("AUTH-001", "No se pudo obtener el usuario", $"the required claim {claimType} not found in token claims."),
                memberName, sourceFilePath, sourceLineNumber
          );
        }
        return claim;
    }

    public string GetUserId([CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = "",
                            [CallerLineNumber] int sourceLineNumber = 0) {
        // Verificar si existe el token
        if (!HasAuthorizationToken()) {
            // Si no hay token, retornar el hostname
            return GetHostName();
        }

        // Si hay token, obtener el claim (si no existe el claim, lanzará excepción)
        return GetRequiredClaimValue(GetJwtToken(), "sub", memberName, sourceFilePath, sourceLineNumber);
    }

    public string GetUserName([CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) {
        // Verificar si existe el token
        if (!HasAuthorizationToken()) {
            // Si no hay token, retornar la IP del cliente
            return GetClientIpAddress();
        }

        // Si hay token, obtener el claim (si no existe el claim, lanzará excepción)
        return GetRequiredClaimValue(GetJwtToken(), "email", memberName, sourceFilePath, sourceLineNumber);
    }

    public string GetToken([CallerMemberName] string memberName = "",
                           [CallerFilePath] string sourceFilePath = "",
                           [CallerLineNumber] int sourceLineNumber = 0) {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new AuthenticationException("HttpContext not found.", memberName, sourceFilePath, sourceLineNumber);
        var authHeader = GetRequiredHeaderValue(httpContext, "Authorization", "Token", memberName, sourceFilePath, sourceLineNumber);

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new CustomException(
                new DictionaryError("AUTH-001", "No se pudo obtener el usuario", "Authorization header is not a valid Bearer token.")
            );
        return authHeader["Bearer ".Length..].Trim();
    }

    public JwtSecurityToken GetJwtToken([CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0) {
        var token = GetToken();
        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new CustomException(
               new DictionaryError("AUTH-001", "No se pudo obtener el usuario", "Token is not a valid JW.")
           );
        return jwtHandler.ReadJwtToken(token);
    }

    private bool HasAuthorizationToken() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return false;

        var authHeader = GetOptionalHeaderValue(httpContext, "Authorization");
        if (string.IsNullOrEmpty(authHeader))
            return false;

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private string GetClientIpAddress() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "unknown";

        // Intentar obtener la IP del encabezado X-Forwarded-For (para proxies/load balancers)
        var forwardedFor = GetOptionalHeaderValue(httpContext, "X-Forwarded-For");
        if (!string.IsNullOrEmpty(forwardedFor)) {
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        // Si no hay X-Forwarded-For, obtener la IP directamente de la conexión
        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress != null) {
            // Si es IPv6 localhost, convertir a IPv4
            if (remoteIpAddress.ToString() == "::1")
                return "127.0.0.1";
            return remoteIpAddress.ToString();
        }

        return "unknown";
    }

    private string GetHostName() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "unknown";

        // Intentar obtener el hostname del encabezado Host
        var host = GetOptionalHeaderValue(httpContext, "Host");
        if (!string.IsNullOrEmpty(host))
            return host;

        // Si no hay Host header, usar el hostname del servidor
        return Environment.MachineName;
    }
}
