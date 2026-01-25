using AppCore.Domain.Enums;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace AppCore.UnitTests.Domain.Enums;

public class PakageLeadTypeTests
{
    [Fact]
    public void PakageLeadType_File_ShouldHaveCorrectValue()
    {
        // Act
        var value = PakageLeadType.File;

        // Assert
        value.Should().Be(PakageLeadType.File);
        ((int)value).Should().Be(0);
    }

    [Fact]
    public void PakageLeadType_Json_ShouldHaveCorrectValue()
    {
        // Act
        var value = PakageLeadType.Json;

        // Assert
        value.Should().Be(PakageLeadType.Json);
        ((int)value).Should().Be(1);
    }

    [Fact]
    public void PakageLeadType_ShouldSerializeAsString()
    {
        // Arrange
        var leadType = PakageLeadType.File;

        // Act
        var json = JsonSerializer.Serialize(leadType);

        // Assert
        json.Should().Be("\"File\"");
    }

    [Fact]
    public void PakageLeadType_ShouldDeserializeFromString()
    {
        // Arrange
        var json = "\"Json\"";

        // Act
        var leadType = JsonSerializer.Deserialize<PakageLeadType>(json);

        // Assert
        leadType.Should().Be(PakageLeadType.Json);
    }
}
