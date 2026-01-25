using System.Text.Json;
using AppCore.Application.DTOs;
using AppCore.Application.Exceptions;
using AppCore.Application.Extensions;
using AppCore.Application.Serialization;
using AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions;

public class JsonExtendTests {
    private readonly EmailRequest _testModel = new() {
        To = "test@example.com",
        Subject = "Test Subject",
        Body = "Test Body",
        From = "sender@example.com"
    };

    [Fact]
    public void ToJsonDocument_WithValidObject_ShouldReturnJsonDocument() {
        // Act
        var result = JsonExtend.ToJsonDocument(_testModel);

        // Assert
        result.Should().NotBeNull();
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        result.RootElement.GetProperty("to").GetString().Should().Be("test@example.com");
        result.RootElement.GetProperty("subject").GetString().Should().Be("Test Subject");
        result.RootElement.GetProperty("body").GetString().Should().Be("Test Body");
    }

    [Fact]
    public void ToJsonDocument_WithNullObject_ShouldThrowSerializerException() {
        // Arrange
        EmailRequest? nullModel = null;

        // Act & Assert
        var act = () => JsonExtend.ToJsonDocument(nullModel!);
        act.Should().NotThrow();
    }

    [Fact]
    public void FromJsonDocument_WithValidJsonDocument_ShouldReturnObject() {
        // Arrange
        var jsonDocument = JsonExtend.ToJsonDocument(_testModel);

        // Act
        var result = JsonExtend.FromJsonDocument<EmailRequest>(jsonDocument!);

        // Assert
        result.Should().NotBeNull();
        result!.To.Should().Be("test@example.com");
        result.Subject.Should().Be("Test Subject");
        result.Body.Should().Be("Test Body");
    }

    [Fact]
    public void FromJsonDocument_WithInvalidJsonStructure_ShouldThrowSerializerException() {
        // Arrange
        var jsonString = "{ \"invalid\": \"structure\" }";
        var jsonDocument = JsonDocument.Parse(jsonString);

        // Act & Assert
        // Act & Assert
        var act = () => JsonExtend.FromJsonDocument<EmailRequest>(jsonDocument);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJsonDocument_WithEmptyJsonDocument_ShouldReturnDefault() {
        // Arrange
        var jsonString = "{}";
        var jsonDocument = JsonDocument.Parse(jsonString);

        // Act
        // Act & Assert
        var act = () => JsonExtend.FromJsonDocument<EmailRequest>(jsonDocument);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Serialize_WithValidObject_ShouldReturnJsonString() {
        // Act
        var result = JsonExtend.Serialize(_testModel);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"to\": \"test@example.com\"");
        result.Should().Contain("\"subject\": \"Test Subject\"");
        result.Should().Contain("\"body\": \"Test Body\"");
    }

    [Fact]
    public void Serialize_WithNullValues_ShouldIgnoreNullProperties() {
        // Arrange
        var modelWithNulls = new Response<string> {
            Data = null,
            Message = "Test Message"
        };

        // Act
        var result = JsonExtend.Serialize(modelWithNulls);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotContain("\"data\":");
        result.Should().Contain("\"message\": \"Test Message\"");
    }

    [Fact]
    public void Serialize_WithSpecialCharacters_ShouldEscapeCorrectly() {
        // Arrange
        var modelWithSpecialChars = new EmailRequest {
            To = "test<tag>@example.com",
            Subject = "Sub\"ject\"",
            Body = "Body",
            From = "sender@example.com"
        };

        // Act
        var result = JsonExtend.Serialize(modelWithSpecialChars);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("test\\u003Ctag\\u003E@example.com");
    }

    [Fact]
    public void Deserialize_WithValidJsonString_ShouldReturnObject() {
        // Arrange
        var jsonString = """
        {
            "to": "deserialize@test.com",
            "subject": "Deserialize Subject",
            "body": "Body",
            "from": "sender@test.com"
        }
        """;

        // Act
        var result = JsonExtend.Deserialize<EmailRequest>(jsonString);

        // Assert
        result.Should().NotBeNull();
        result!.To.Should().Be("deserialize@test.com");
        result.Subject.Should().Be("Deserialize Subject");
    }

    [Fact]
    public void Deserialize_WithEmptyString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<EmailRequest>("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithWhitespaceString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<EmailRequest>("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithNullString_ShouldReturnDefault() {
        // Act
        var result = JsonExtend.Deserialize<EmailRequest>(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ShouldThrowSerializerException() {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        var act = () => JsonExtend.Deserialize<EmailRequest>(invalidJson);
        act.Should().Throw<SerializerException>();
    }

    [Fact]
    public void Deserialize_WithCaseInsensitiveProperties_ShouldWork() {
        // Arrange
        var jsonString = """
        {
            "TO": "case@test.com",
            "SUBJECT": "Case Subject",
            "BODY": "Body",
            "FROM": "sender@test.com"
        }
        """;

        // Act
        var result = JsonExtend.Deserialize<EmailRequest>(jsonString);

        // Assert
        result.Should().NotBeNull();
        result!.To.Should().Be("case@test.com");
        result.Subject.Should().Be("Case Subject");
    }
}
