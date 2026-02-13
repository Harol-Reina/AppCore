using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class PaginationResponseTests {
    [Fact]
    public void Constructor_WithData_ShouldSetPropertiesCorrectly() {
        // Arrange
        var data = new List<string> { "item1", "item2", "item3" };

        // Act
        var response = new PaginationResponse<string>(data);

        // Assert
        response.Results.Should().HaveCount(3);
        response.Count.Should().Be(3);
        response.Pages.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNullData_ShouldInitializeEmptyList() {
        // Act
        var response = new PaginationResponse<string>(null);

        // Assert
        response.Results.Should().BeEmpty();
        response.Count.Should().Be(0);
    }

    [Fact]
    public void Success_StaticMethod_ShouldReturnPaginationResponse() {
        // Arrange
        var data = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var response = PaginationResponse<int>.Success(data);

        // Assert
        response.Should().NotBeNull();
        response.Results.Should().HaveCount(5);
        response.Count.Should().Be(5);
    }

    [Fact]
    public void Properties_ShouldBeSettable() {
        // Arrange
        var response = new PaginationResponse<string>();

        // Act
        response.Count = 100;
        response.Pages = 10;
        response.Results = new List<string> { "a", "b", "c" };

        // Assert
        response.Count.Should().Be(100);
        response.Pages.Should().Be(10);
        response.Results.Should().HaveCount(3);
    }

    [Fact]
    public void WithComplexType_ShouldWork() {
        // Arrange
        var items = new List<LoginRequest>
        {
            new LoginRequest("user1", "pass1"),
            new LoginRequest("user2", "pass2")
        };

        // Act
        var response = new PaginationResponse<LoginRequest> {
            Count = 2,
            Pages = 1,
            Results = items
        };

        // Assert
        response.Count.Should().Be(2);
        response.Results.Should().HaveCount(2);
        response.Results[0].UserName.Should().Be("user1");
    }
}
