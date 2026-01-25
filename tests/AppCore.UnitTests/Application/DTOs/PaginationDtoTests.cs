using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class PaginationDtoTests
{
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var data = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var dto = new PaginationDto<string>
        {
            Count = 3,
            Pages = 1,
            Results = data
        };

        // Assert
        dto.Count.Should().Be(3);
        dto.Pages.Should().Be(1);
        dto.Results.Should().HaveCount(3);
        dto.Results.Should().Contain("Item2");
    }

    [Fact]
    public void Results_DefaultValue_ShouldBeEmptyList()
    {
        // Act
        var dto = new PaginationDto<int>();

        // Assert
        dto.Results.Should().NotBeNull();
        dto.Results.Should().BeEmpty();
    }

    [Fact]
    public void WithComplexType_ShouldWork()
    {
        // Arrange
        var items = new List<LoginRequest>
        {
            new LoginRequest("user1", "pass1"),
            new LoginRequest("user2", "pass2")
        };

        // Act
        var dto = new PaginationDto<LoginRequest>
        {
            Count = 2,
            Pages = 1,
            Results = items
        };

        // Assert
        dto.Count.Should().Be(2);
        dto.Results.Should().HaveCount(2);
        dto.Results!.First().UserName.Should().Be("user1");
    }

    [Fact]
    public void Results_CanBeNull()
    {
        // Act
        var dto = new PaginationDto<string>
        {
            Count = 0,
            Pages = 0,
            Results = null
        };

        // Assert
        dto.Results.Should().BeNull();
    }
}
