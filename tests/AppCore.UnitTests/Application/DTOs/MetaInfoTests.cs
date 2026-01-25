using AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.DTOs;

public class MetaInfoTests
{
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dateCompile = new DateTime(2024, 1, 15);

        // Act
        var metaInfo = new MetaInfo
        {
            Environment = "Production",
            Version = "1.0.0",
            DateCompile = dateCompile,
            TableName = "MyDatabase",
            EndPoinds = new List<string> { "/api/users", "/api/orders" }
        };

        // Assert
        metaInfo.Environment.Should().Be("Production");
        metaInfo.Version.Should().Be("1.0.0");
        metaInfo.DateCompile.Should().Be(dateCompile);
        metaInfo.TableName.Should().Be("MyDatabase");
        metaInfo.EndPoinds.Should().HaveCount(2);
        metaInfo.EndPoinds.Should().Contain(new[] { "/api/users", "/api/orders" });
    }

    [Fact]
    public void OptionalProperties_ShouldAcceptNull()
    {
        // Act
        var metaInfo = new MetaInfo
        {
            Environment = null,
            Version = null,
            DateCompile = DateTime.UtcNow,
            TableName = null,
            EndPoinds = null
        };

        // Assert
        metaInfo.Environment.Should().BeNull();
        metaInfo.Version.Should().BeNull();
        metaInfo.TableName.Should().BeNull();
        metaInfo.EndPoinds.Should().BeNull();
    }
}
