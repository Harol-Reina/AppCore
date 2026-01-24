namespace AppCore.Application.DTOs;

public class LoginRequest {
    /// <summary>Usuario</summary>
    public string UserName { get; set; } = null!;

    /// <summary>Contraseña de acceso</summary>
    public string PassWord { get; set; } = null!;
}
