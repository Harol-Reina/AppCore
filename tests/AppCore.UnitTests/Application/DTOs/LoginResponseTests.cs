using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class LoginResponseTests {
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly() {
        // Arrange & Act
        var response = new LoginResponse {
            UserName = "testuser",
            FullName = "Test User",
            Roles = new List<string> { "Admin", "User" },
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            RefreshToken = "refresh-token-123",
            ReinsuredCompanyId = 42
        };

        // Assert
        response.UserName.Should().Be("testuser");
        response.FullName.Should().Be("Test User");
        response.Roles.Should().HaveCount(2);
        response.Roles.Should().Contain(new[] { "Admin", "User" });
        response.Token.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9");
        response.RefreshToken.Should().Be("refresh-token-123");
        response.ReinsuredCompanyId.Should().Be(42);
    }

    [Fact]
    public void ReinsuredCompanyId_ShouldBeNullable() {
        // Arrange & Act
        var response = new LoginResponse {
            UserName = "test",
            FullName = "Test",
            Roles = new List<string>(),
            Token = "token",
            RefreshToken = "refresh",
            ReinsuredCompanyId = null
        };

        // Assert
        response.ReinsuredCompanyId.Should().BeNull();
    }
}
