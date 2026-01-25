using AppCore.Application.Serialization;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace AppCore.UnitTests.Application.Serialization;

public class JsonTestModelTests
{
    [Fact]
    public void JsonTestModel_Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var model = new JsonTestModel
        {
            Id = 123,
            Name = "Test Name",
            Email = "test@example.com",
            IsActive = true
        };

        // Assert
        model.Id.Should().Be(123);
        model.Name.Should().Be("Test Name");
        model.Email.Should().Be("test@example.com");
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public void JsonTestModel_ShouldSerializeToJson()
    {
        // Arrange
        var model = new JsonTestModel
        {
            Id = 456,
            Name = "Serialization Test",
            Email = "serialize@test.com",
            IsActive = false
        };

        // Act
        var json = JsonSerializer.Serialize(model, AppCoreJsonContext.Default.JsonTestModel);

        // Assert
        json.Should().Contain("\"id\": 456");
        json.Should().Contain("\"name\": \"Serialization Test\"");
        json.Should().Contain("\"email\": \"serialize@test.com\"");
        json.Should().Contain("\"isActive\": false");
    }

    [Fact]
    public void JsonTestModel_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """
        {
            "id": 789,
            "name": "Deserialization Test",
            "email": "deserialize@test.com",
            "isActive": true
        }
        """;

        // Act
        var model = JsonSerializer.Deserialize(json, AppCoreJsonContext.Default.JsonTestModel);

        // Assert
        model.Should().NotBeNull();
        model!.Id.Should().Be(789);
        model.Name.Should().Be("Deserialization Test");
        model.Email.Should().Be("deserialize@test.com");
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public void JsonTestModel_WithNullValues_ShouldSerializeWithoutNulls()
    {
        // Arrange
        var model = new JsonTestModel
        {
            Id = 100,
            Name = null,
            Email = null,
            IsActive = false
        };

        // Act
        var json = JsonSerializer.Serialize(model, AppCoreJsonContext.Default.JsonTestModel);

        // Assert
        json.Should().Contain("\"id\": 100");
        json.Should().NotContain("\"name\"");
        json.Should().NotContain("\"email\"");
    }

    [Fact]
    public void JsonTestModel_RoundTrip_ShouldPreserveValues()
    {
        // Arrange
        var original = new JsonTestModel
        {
            Id = 999,
            Name = "Round Trip",
            Email = "roundtrip@test.com",
            IsActive = true
        };

        // Act
        var json = JsonSerializer.Serialize(original, AppCoreJsonContext.Default.JsonTestModel);
        var deserialized = JsonSerializer.Deserialize(json, AppCoreJsonContext.Default.JsonTestModel);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.Name.Should().Be(original.Name);
        deserialized.Email.Should().Be(original.Email);
        deserialized.IsActive.Should().Be(original.IsActive);
    }
}
