using OrionSoft.AppCore.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.DTOs;

public class MetaInfoTests {
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly() {
        // Arrange
        var dateCompile = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var metaInfo = new MetaInfo {
            Environment = "Production",
            Version = "1.0.0",
            DateCompile = dateCompile,
            TableName = "MyDatabase",
            Endpoints = new List<string> { "/api/users", "/api/orders" }
        };

        // Assert
        metaInfo.Environment.Should().Be("Production");
        metaInfo.Version.Should().Be("1.0.0");
        metaInfo.DateCompile.Should().Be(dateCompile);
        metaInfo.TableName.Should().Be("MyDatabase");
        metaInfo.Endpoints.Should().HaveCount(2);
        metaInfo.Endpoints.Should().Contain(new[] { "/api/users", "/api/orders" });
    }

    [Fact]
    public void OptionalProperties_ShouldAcceptNull() {
        // Act
        var metaInfo = new MetaInfo {
            Environment = null,
            Version = null,
            DateCompile = DateTime.UtcNow,
            TableName = null,
            Endpoints = null
        };

        // Assert
        metaInfo.Environment.Should().BeNull();
        metaInfo.Version.Should().BeNull();
        metaInfo.TableName.Should().BeNull();
        metaInfo.Endpoints.Should().BeNull();
    }
}
