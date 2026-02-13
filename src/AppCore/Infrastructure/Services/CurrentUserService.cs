using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace OrionSoft.AppCore.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService {
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private static string? GetOptionalHeaderValue(HttpContext httpContext, string headerName) {
        if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValues)) {
            return headerValues.FirstOrDefault()?.Trim();
        }
        return null; // Returns null if the header does not exist
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

    // Method to get a required header value
    private static string GetRequiredHeaderValue(HttpContext httpContext, string headerName, string displayName, [CallerMemberName] string memberName = "",
                                                 [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
        var value = GetOptionalHeaderValue(httpContext, headerName);
        if (string.IsNullOrEmpty(value)) {
            // Used for storing in the database with createdby and updatedby
            throw new AuthenticationException($"{displayName} not found in headers.", memberName, sourceFilePath, sourceLineNumber);
        }
        return value;
    }

    // Method to get a required claim value
    private static string GetRequiredClaimValue(JwtSecurityToken token, string claimType, [CallerMemberName] string memberName = "",
                                                [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
        var claim = token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrEmpty(claim)) {
            throw new AuthenticationException(
                $"Required claim '{claimType}' not found in token claims.",
                memberName, sourceFilePath, sourceLineNumber
            );
        }
        return claim;
    }

    public string GetUserId([CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = "",
                            [CallerLineNumber] int sourceLineNumber = 0) {
        // Check if a token exists
        if (!HasAuthorizationToken()) {
            // If no token, return the hostname
            return GetHostName();
        }

        // If a token exists, get the claim (throws if the claim is missing)
        return GetRequiredClaimValue(GetJwtToken(), "sub", memberName, sourceFilePath, sourceLineNumber);
    }

    public string GetUserName([CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0) {
        // Check if a token exists
        if (!HasAuthorizationToken()) {
            // If no token, return the client IP address
            return GetClientIpAddress();
        }

        // If a token exists, get the claim (throws if the claim is missing)
        return GetRequiredClaimValue(GetJwtToken(), "email", memberName, sourceFilePath, sourceLineNumber);
    }

    public string GetToken([CallerMemberName] string memberName = "",
                           [CallerFilePath] string sourceFilePath = "",
                           [CallerLineNumber] int sourceLineNumber = 0) {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new AuthenticationException("HttpContext not found.", memberName, sourceFilePath, sourceLineNumber);
        var authHeader = GetRequiredHeaderValue(httpContext, "Authorization", "Token", memberName, sourceFilePath, sourceLineNumber);

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new AuthenticationException(
                "Authorization header is not a valid Bearer token.",
                memberName, sourceFilePath, sourceLineNumber
            );
        return authHeader["Bearer ".Length..].Trim();
    }

    public JwtSecurityToken GetJwtToken([CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0) {
        var token = GetToken();
        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new AuthenticationException(
                "Token is not a valid JWT.",
                memberName, sourceFilePath, sourceLineNumber
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

        // Try to get the IP from the X-Forwarded-For header (for proxies/load balancers)
        var forwardedFor = GetOptionalHeaderValue(httpContext, "X-Forwarded-For");
        if (!string.IsNullOrEmpty(forwardedFor)) {
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        // If no X-Forwarded-For, get the IP directly from the connection
        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress != null) {
            // If IPv6 localhost, convert to IPv4
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

        // Try to get the hostname from the Host header
        var host = GetOptionalHeaderValue(httpContext, "Host");
        if (!string.IsNullOrEmpty(host))
            return host;

        // If no Host header, use the server hostname
        return Environment.MachineName;
    }
}
