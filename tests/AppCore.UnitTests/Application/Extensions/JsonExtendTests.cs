using System.Text.Json;
using AppCore.Application.Extensions;
using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions;

public class JsonExtendTests {
    private readonly TestModel _testModel = new() {
        Id = 123,
        Name = "Test Name",
        Email = "test@example.com",
        IsActive = true
    };

    [Fact]
    public void ToJsonDocument_WithValidObject_ShouldReturnJsonDocument() {
        // Act
        var result = JsonExtend.ToJsonDocument(_testModel);

        // Assert
        result.Should().NotBeNull();
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        result.RootElement.GetProperty("id").GetInt32().Should().Be(123);
        result.RootElement.GetProperty("name").GetString().Should().Be("Test Name");
        result.RootElement.GetProperty("email").GetString().Should().Be("test@example.com");
        result.RootElement.GetProperty("isActive").GetBoolean().Should().Be(true);
    }

    [Fact]
    public void ToJsonDocument_WithNullObject_ShouldThrowSerializerException() {
        // Arrange
        TestModel? nullModel = null;

        // Act & Assert
        var act = () => JsonExtend.ToJsonDocument(nullModel!);
        act.Should().NotThrow();
    }

    [Fact]
    public void FromJsonDocument_WithValidJsonDocument_ShouldReturnObject() {
        // Arrange
        var jsonDocument = JsonExtend.ToJsonDocument(_testModel);

        // Act
        var result = JsonExtend.FromJsonDocument<TestModel>(jsonDocument!);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Name.Should().Be("Test Name");
        result.Email.Should().Be("test@example.com");
        result.IsActive.Should().Be(true);
    }

    [Fact]
    public void FromJsonDocument_WithInvalidJsonStructure_ShouldThrowSerializerException() {
        // Arrange
        var jsonString = "{ \"invalid\": \"structure\" }";
        var jsonDocument = JsonDocument.Parse(jsonString);

        // Act & Assert
        var act = () => JsonExtend.FromJsonDocument<TestModel>(jsonDocument);
        act.Should().NotThrow();
    }

    [Fact]
    public void FromJsonDocument_WithEmptyJsonDocument_ShouldReturnDefault() {
        // Arrange
        var jsonString = "{}";
        var jsonDocument = JsonDocument.Parse(jsonString);

        // Act
        var result = JsonExtend.FromJsonDocument<TestModel>(jsonDocument);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(0);
        result.Name.Should().BeNull();
        result.Email.Should().BeNull();
        result.IsActive.Should().Be(false);
    }

    [Fact]
    public void Serialize_WithValidObject_ShouldReturnJsonString() {
        // Act
        var result = JsonExtend.Serialize(_testModel);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"id\": 123");
        result.Should().Contain("\"name\": \"Test Name\"");
        result.Should().Contain("\"email\": \"test@example.com\"");
        result.Should().Contain("\"isActive\": true");
    }

    [Fact]
    public void Serialize_WithNullValues_ShouldIgnoreNullProperties() {
        // Arrange
        var modelWithNulls = new TestModel {
            Id = 456,
            Name = null,
            Email = "test@example.com",
            IsActive = false
        };

        // Act
        var result = JsonExtend.Serialize(modelWithNulls);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"id\": 456");
        result.Should().NotContain("\"name\":");
        result.Should().Contain("\"email\": \"test@example.com\"");
        result.Should().Contain("\"isActive\": false");
    }

    [Fact]
    public void Serialize_WithSpecialCharacters_ShouldEscapeCorrectly() {
        // Arrange
        var modelWithSpecialChars = new TestModel {
            Id = 789,
            Name = "Test with \"quotes\" and <tags>",
            Email = "user@domain.com",
            IsActive = true
        };

        // Act
        var result = JsonExtend.Serialize(modelWithSpecialChars);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Test with \\\"quotes\\\" and <tags>");
    }

    [Fact]
    public void Deserialize_WithValidJsonString_ShouldReturnObject() {
        // Arrange
        var jsonString = """
        {
            "id": 999,
            "name": "Deserialized Test",
            "email": "deserialize@test.com",
            "isActive": false
        }
        """;

        // Act
        var result = JsonExtend.Deserialize<TestModel>(jsonString);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(999);
        result.Name.Should().Be("Deserialized Test");
        result.Email.Should().Be("deserialize@test.com");
        result.IsActive.Should().Be(false);
    }

    [Fact]
    public void Deserialize_WithEmptyString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<TestModel>("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithWhitespaceString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<TestModel>("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithNullString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<TestModel>(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ShouldThrowSerializerException() {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        var act = () => JsonExtend.Deserialize<TestModel>(invalidJson);
        act.Should().Throw<SerializerException>();
    }

    [Fact]
    public void Deserialize_WithCaseInsensitiveProperties_ShouldWork() {
        // Arrange
        var jsonString = """
        {
            "ID": 777,
            "NAME": "Case Insensitive Test",
            "EMAIL": "case@test.com",
            "ISACTIVE": true
        }
        """;

        // Act
        var result = JsonExtend.Deserialize<TestModel>(jsonString);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(777);
        result.Name.Should().Be("Case Insensitive Test");
        result.Email.Should().Be("case@test.com");
        result.IsActive.Should().Be(true);
    }

    [Fact]
    public void Serialize_WithCircularReference_ShouldHandleGracefully() {
        // Arrange
        var parent = new CircularTestModel { Name = "Parent" };
        var child = new CircularTestModel { Name = "Child", Parent = parent };
        parent.Child = child;

        // Act & Assert
        var act = () => JsonExtend.Serialize(parent);
        act.Should().Throw<SerializerException>();
    }

    public class TestModel {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class CircularTestModel {
        public string? Name { get; set; }
        public CircularTestModel? Parent { get; set; }
        public CircularTestModel? Child { get; set; }
    }
}
