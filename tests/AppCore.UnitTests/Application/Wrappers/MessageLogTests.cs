using System.Text.Json;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Wrappers;

public class MessageLogTests {
    [Fact]
    public void MessageLog_RequiredProperties_ShouldBeSetCorrectly() {
        // Arrange
        var type = "ERROR";
        var message = "Test error message";
        var method = "TestMethod";
        var path = "/api/test";

        // Act
        var messageLog = new MessageLog {
            Type = type,
            Message = JsonExtend.ToJsonElement(message),
            Method = method,
            Path = path
        };

        // Assert
        messageLog.Type.Should().Be(type);
        messageLog.Message.GetString().Should().Be(message);
        messageLog.Method.Should().Be(method);
        messageLog.Path.Should().Be(path);
    }

    [Fact]
    public void MessageLog_OptionalProperties_ShouldBeSetCorrectly() {
        // Arrange
        var source = "TestSource";
        var stackTrace = "   at TestClass.TestMethod() in TestFile.cs:line 10";

        // Act
        var messageLog = new MessageLog {
            Type ="INFO",
            Message = JsonExtend.ToJsonElement("Test message"),
            Method ="TestMethod",
            Path = "/api/info",
            Source = source,
            StackTrace = stackTrace
        };

        // Assert
        messageLog.Source.Should().Be(source);
        messageLog.StackTrace.Should().Be(stackTrace);
    }

    [Fact]
    public void MessageLog_WithNullOptionalProperties_ShouldAllowNullValues() {
        // Act
        var messageLog = new MessageLog {
            Type ="DEBUG",
            Message = JsonExtend.ToJsonElement("Debug message"),
            Method ="DebugMethod",
            Path = "/api/debug",
            Source = null,
            StackTrace = null
        };

        // Assert
        messageLog.Source.Should().BeNull();
        messageLog.StackTrace.Should().BeNull();
    }

    [Fact]
    public void MessageLog_WithComplexMessage_ShouldHandleJsonElement() {
        // Arrange
        var complexJson = JsonDocument.Parse("""{"id":123,"description":"Complex error occurred","details":["Detail1","Detail2"]}""");

        // Act
        var messageLog = new MessageLog {
            Type ="ERROR",
            Message = complexJson.RootElement.Clone(),
            Method ="ProcessData",
            Path = "/api/process"
        };

        // Assert
        messageLog.Message.ValueKind.Should().Be(JsonValueKind.Object);
        messageLog.Message.GetProperty("id").GetInt32().Should().Be(123);
        messageLog.Message.GetProperty("description").GetString().Should().Be("Complex error occurred");
    }

    [Fact]
    public void MessageLog_ToString_ShouldReturnSerializedJson() {
        // Arrange
        var messageLog = new MessageLog {
            Type ="INFO",
            Source = "TestController",
            Message = JsonExtend.ToJsonElement("Operation completed successfully"),
            Method ="GetData",
            Path = "/api/data",
            StackTrace = null
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"type\": \"INFO\"");
        result.Should().Contain("\"source\": \"TestController\"");
        result.Should().Contain("\"message\": \"Operation completed successfully\"");
        result.Should().Contain("\"method\": \"GetData\"");
        result.Should().Contain("\"path\": \"/api/data\"");
    }

    [Fact]
    public void MessageLog_ToString_WithComplexMessage_ShouldSerializeCorrectly() {
        // Arrange
        var requestId = Guid.NewGuid();
        var messageData = new Dictionary<string, object> {
            { "errorCode", "ERR001" },
            { "userAction", "Login" },
            { "requestId", requestId }
        };

        var messageLog = new MessageLog {
            Type ="ERROR",
            Message = JsonExtend.ToJsonElement(messageData),
            Method ="Login",
            Path = "/auth/login",
            StackTrace = "Stack trace content"
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"type\": \"ERROR\"");
        result.Should().Contain("\"errorCode\": \"ERR001\"");
        result.Should().Contain("\"userAction\": \"Login\"");
        result.Should().Contain("\"method\": \"Login\"");
        result.Should().Contain("\"path\": \"/auth/login\"");
        result.Should().Contain("\"stackTrace\": \"Stack trace content\"");
    }

    [Fact]
    public void MessageLog_ToString_WithNullOptionalFields_ShouldHandleGracefully() {
        // Arrange
        var messageLog = new MessageLog {
            Type ="WARNING",
            Message = JsonExtend.ToJsonElement("Warning message"),
            Method ="ValidateInput",
            Path = "/api/validate"
            // Source and StackTrace are null by default
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"type\": \"WARNING\"");
        result.Should().Contain("\"message\": \"Warning message\"");
        result.Should().Contain("\"method\": \"ValidateInput\"");
        result.Should().Contain("\"path\": \"/api/validate\"");
    }

    [Fact]
    public void MessageLog_Record_ShouldSupportEquality() {
        // Arrange — use the same JsonElement instance for equality
        var message = JsonExtend.ToJsonElement("Test message");
        var messageLog1 = new MessageLog {
            Type ="INFO",
            Message = message,
            Method ="TestMethod",
            Path = "/test"
        };

        var messageLog2 = new MessageLog {
            Type ="INFO",
            Message = message,
            Method ="TestMethod",
            Path = "/test"
        };

        var messageLog3 = new MessageLog {
            Type ="ERROR",
            Message = message,
            Method ="TestMethod",
            Path = "/test"
        };

        // Act & Assert
        messageLog1.Should().Be(messageLog2);
        messageLog1.Should().NotBe(messageLog3);
        messageLog1.GetHashCode().Should().Be(messageLog2.GetHashCode());
    }

    [Fact]
    public void MessageLog_DifferentTypes_ShouldAllWork() {
        // Arrange & Act
        var logTypes = new[]
        {
            new MessageLog { Type ="INFO", Message = JsonExtend.ToJsonElement("Info message"), Method ="Method1", Path = "/path1" },
            new MessageLog { Type ="DEBUG", Message = JsonExtend.ToJsonElement("Debug message"), Method ="Method2", Path = "/path2" },
            new MessageLog { Type ="WARNING", Message = JsonExtend.ToJsonElement("Warning message"), Method ="Method3", Path = "/path3" },
            new MessageLog { Type ="ERROR", Message = JsonExtend.ToJsonElement("Error message"), Method ="Method4", Path = "/path4" },
            new MessageLog { Type ="FATAL", Message = JsonExtend.ToJsonElement("Fatal message"), Method ="Method5", Path = "/path5" }
        };

        // Assert
        foreach (var log in logTypes) {
            log.ToString().Should().NotBeNullOrWhiteSpace();
            log.Type.Should().NotBeNullOrEmpty();
            log.Message.GetString().Should().NotBeNullOrEmpty();
            log.Method.Should().NotBeNullOrEmpty();
            log.Path.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void MessageLog_WithStringMessage_ShouldWorkCorrectly() {
        // Arrange
        var stringMessage = "Simple string message";

        // Act
        var messageLog = new MessageLog {
            Type ="INFO",
            Message = JsonExtend.ToJsonElement(stringMessage),
            Method ="StringTest",
            Path = "/string/test"
        };

        // Assert
        messageLog.Message.GetString().Should().Be(stringMessage);
        messageLog.ToString().Should().Contain($"\"message\": \"{stringMessage}\"");
    }

    [Fact]
    public void MessageLog_WithNumericMessage_ShouldWorkCorrectly() {
        // Arrange
        var numericMessage = 42;

        // Act
        var messageLog = new MessageLog {
            Type ="DEBUG",
            Message = JsonExtend.ToJsonElement(numericMessage),
            Method ="NumericTest",
            Path = "/numeric/test"
        };

        // Assert
        messageLog.Message.GetInt32().Should().Be(numericMessage);
        messageLog.ToString().Should().Contain("\"message\": 42");
    }
}
