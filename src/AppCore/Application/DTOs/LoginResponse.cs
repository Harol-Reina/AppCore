namespace AppCore.Application.DTOs;

public class LoginResponse {
    /// <summary>Usuario logueado.</summary>
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public required IList<string> Roles { get; set; }

    /// <summary>Token de acceso.</summary>
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the reinsured company.
    /// </summary>
    /// <value>
    /// The identifier of the reinsured company, or <c>null</c> if not applicable.
    /// </value>
    public int? ReinsuredCompanyId { get; set; }
}
