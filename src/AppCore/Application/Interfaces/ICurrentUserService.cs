using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;

namespace AppCore.Application.Interfaces;

public interface ICurrentUserService {

    string GetUserId([CallerMemberName] string memberName = "",
                     [CallerFilePath] string sourceFilePath = "",
                     [CallerLineNumber] int sourceLineNumber = 0);

    string GetUserName([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0);

    string GetToken([CallerMemberName] string memberName = "",
                    [CallerFilePath] string sourceFilePath = "",
                    [CallerLineNumber] int sourceLineNumber = 0);

    JwtSecurityToken GetJwtToken([CallerMemberName] string memberName = "",
                    [CallerFilePath] string sourceFilePath = "",
                    [CallerLineNumber] int sourceLineNumber = 0);
                    
    string GetXtraceId([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0);
}
