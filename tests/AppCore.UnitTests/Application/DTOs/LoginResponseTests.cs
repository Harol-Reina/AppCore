using OrionSoft.AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.DTOs;

public class LoginResponseTests {
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly() {
        // Arrange & Act
        var response = new LoginResponse {
            UserName = "testuser",
            FullName = "Test User",
            Roles = new List<string> { "Admin", "User" },
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            RefreshToken = "refresh-token-123"
        };

        // Assert
        response.UserName.Should().Be("testuser");
        response.FullName.Should().Be("Test User");
        response.Roles.Should().HaveCount(2);
        response.Roles.Should().Contain(new[] { "Admin", "User" });
        response.Token.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9");
        response.RefreshToken.Should().Be("refresh-token-123");
    }
}
