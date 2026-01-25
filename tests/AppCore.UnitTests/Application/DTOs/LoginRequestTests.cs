using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class LoginRequestTests {
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties() {
        // Arrange & Act
        var loginRequest = new LoginRequest("testuser", "testpass123");

        // Assert
        loginRequest.UserName.Should().Be("testuser");
        loginRequest.PassWord.Should().Be("testpass123");
    }

    [Fact]
    public void Properties_ShouldBeSettable() {
        // Arrange
        var loginRequest = new LoginRequest("olduser", "oldpass");

        // Act
        loginRequest.UserName = "newuser";
        loginRequest.PassWord = "newpass456";

        // Assert
        loginRequest.UserName.Should().Be("newuser");
        loginRequest.PassWord.Should().Be("newpass456");
    }
}
