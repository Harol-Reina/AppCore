namespace AppCore.Application.DTOs;

public sealed class LoginRequest(string userName, string passWord) {
    /// <summary>Usuario</summary>
    public string UserName { get; set; } = userName;

    /// <summary>Contraseña de acceso</summary>
    public string PassWord { get; set; } = passWord;
}
