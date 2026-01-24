namespace AppCore.Application.Settings;

public class JWTSettings {
    public string Secret { get; set; }  = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public double DurationInMinutes { get; set; }
}
