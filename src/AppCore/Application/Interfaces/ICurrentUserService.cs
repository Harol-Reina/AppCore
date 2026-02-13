using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;

namespace OrionSoft.AppCore.Application.Interfaces;

/// <summary>
/// Provides access to current authenticated user information.
/// </summary>
public interface ICurrentUserService {

    /// <summary>
    /// Gets the current user's unique identifier.
    /// </summary>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    /// <returns>The current user's ID as a string.</returns>
    string GetUserId([CallerMemberName] string memberName = "",
                     [CallerFilePath] string sourceFilePath = "",
                     [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Gets the current user's username.
    /// </summary>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    /// <returns>The current user's username as a string.</returns>
    string GetUserName([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Gets the current user's JWT token as a raw string.
    /// </summary>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    /// <returns>The JWT token as a string.</returns>
    string GetToken([CallerMemberName] string memberName = "",
                    [CallerFilePath] string sourceFilePath = "",
                    [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Gets the current user's JWT token as a parsed JwtSecurityToken object.
    /// </summary>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    /// <returns>The parsed JwtSecurityToken object.</returns>
    JwtSecurityToken GetJwtToken([CallerMemberName] string memberName = "",
                    [CallerFilePath] string sourceFilePath = "",
                    [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Gets the current request's trace identifier for correlation.
    /// </summary>
    /// <param name="memberName">The name of the calling member (auto-populated).</param>
    /// <param name="sourceFilePath">The source file path of the calling member (auto-populated).</param>
    /// <param name="sourceLineNumber">The line number of the calling member (auto-populated).</param>
    /// <returns>The X-Trace-ID for the current request.</returns>
    string GetXtraceId([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0);
}
