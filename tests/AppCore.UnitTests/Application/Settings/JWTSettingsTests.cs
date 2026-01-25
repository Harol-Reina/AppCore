using AppCore.Application.Settings;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Settings;

public class JWTSettingsTests
{
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        var jwtSettings = new JWTSettings
        {
            Secret = "my-super-secret-key-for-jwt-token-at-least-32-characters",
            Issuer = "MyAppIssuer",
            Audience = "MyAppAudience",
            DurationInMinutes = 60.5
        };

        // Assert
        jwtSettings.Secret.Should().Be("my-super-secret-key-for-jwt-token-at-least-32-characters");
        jwtSettings.Issuer.Should().Be("MyAppIssuer");
        jwtSettings.Audience.Should().Be("MyAppAudience");
        jwtSettings.DurationInMinutes.Should().Be(60.5);
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var jwtSettings = new JWTSettings();

        // Assert
        jwtSettings.Should().NotBeNull();
    }
}
