using AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Wrappers;

public class MessageLogTests {
    [Fact]
    public void MessageLog_RequiredProperties_ShouldBeSetCorrectly() {
        // Arrange
        var tipo = "ERROR";
        var message = "Test error message";
        var metodo = "TestMethod";
        var path = "/api/test";

        // Act
        var messageLog = new MessageLog {
            Tipo = tipo,
            Message = message,
            Metodo = metodo,
            Path = path
        };

        // Assert
        messageLog.Tipo.Should().Be(tipo);
        ((string)messageLog.Message).Should().Be(message);
        messageLog.Metodo.Should().Be(metodo);
        messageLog.Path.Should().Be(path);
    }

    [Fact]
    public void MessageLog_OptionalProperties_ShouldBeSetCorrectly() {
        // Arrange
        var source = "TestSource";
        var stackTrace = "   at TestClass.TestMethod() in TestFile.cs:line 10";

        // Act
        var messageLog = new MessageLog {
            Tipo = "INFO",
            Message = "Test message",
            Metodo = "TestMethod",
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
            Tipo = "DEBUG",
            Message = "Debug message",
            Metodo = "DebugMethod",
            Path = "/api/debug",
            Source = null,
            StackTrace = null
        };

        // Assert
        messageLog.Source.Should().BeNull();
        messageLog.StackTrace.Should().BeNull();
    }

    [Fact]
    public void MessageLog_WithComplexMessage_ShouldHandleDynamicMessage() {
        // Arrange
        var complexMessage = new {
            Id = 123,
            Description = "Complex error occurred",
            Details = new[] { "Detail1", "Detail2" },
            Timestamp = DateTime.Now
        };

        // Act
        var messageLog = new MessageLog {
            Tipo = "ERROR",
            Message = complexMessage,
            Metodo = "ProcessData",
            Path = "/api/process"
        };

        // Assert
        Assert.NotNull(messageLog.Message); // Cannot use FluentAssertions on dynamic
        // Complex object stored as dynamic, can't use BeEquivalentTo on dynamic
        Assert.True(messageLog.Message != null);
    }

    [Fact]
    public void MessageLog_ToString_ShouldReturnSerializedJson() {
        // Arrange
        var messageLog = new MessageLog {
            Tipo = "INFO",
            Source = "TestController",
            Message = "Operation completed successfully",
            Metodo = "GetData",
            Path = "/api/data",
            StackTrace = null
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"Tipo\": \"INFO\"");
        result.Should().Contain("\"Source\": \"TestController\"");
        result.Should().Contain("\"Message\": \"Operation completed successfully\"");
        result.Should().Contain("\"Metodo\": \"GetData\"");
        result.Should().Contain("\"Path\": \"/api/data\"");
    }

    [Fact]
    public void MessageLog_ToString_WithComplexMessage_ShouldSerializeCorrectly() {
        // Arrange
        var messageData = new {
            ErrorCode = "ERR001",
            UserAction = "Login",
            RequestId = Guid.NewGuid()
        };

        var messageLog = new MessageLog {
            Tipo = "ERROR",
            Message = messageData,
            Metodo = "Login",
            Path = "/auth/login",
            StackTrace = "Stack trace content"
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"Tipo\": \"ERROR\"");
        result.Should().Contain("\"ErrorCode\": \"ERR001\"");
        result.Should().Contain("\"UserAction\": \"Login\"");
        result.Should().Contain("\"Metodo\": \"Login\"");
        result.Should().Contain("\"Path\": \"/auth/login\"");
        result.Should().Contain("\"StackTrace\": \"Stack trace content\"");
    }

    [Fact]
    public void MessageLog_ToString_WithNullOptionalFields_ShouldHandleGracefully() {
        // Arrange
        var messageLog = new MessageLog {
            Tipo = "WARNING",
            Message = "Warning message",
            Metodo = "ValidateInput",
            Path = "/api/validate"
            // Source and StackTrace are null by default
        };

        // Act
        var result = messageLog.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"Tipo\": \"WARNING\"");
        result.Should().Contain("\"Message\": \"Warning message\"");
        result.Should().Contain("\"Metodo\": \"ValidateInput\"");
        result.Should().Contain("\"Path\": \"/api/validate\"");
    }

    [Fact]
    public void MessageLog_Record_ShouldSupportEquality() {
        // Arrange
        var messageLog1 = new MessageLog {
            Tipo = "INFO",
            Message = "Test message",
            Metodo = "TestMethod",
            Path = "/test"
        };

        var messageLog2 = new MessageLog {
            Tipo = "INFO",
            Message = "Test message",
            Metodo = "TestMethod",
            Path = "/test"
        };

        var messageLog3 = new MessageLog {
            Tipo = "ERROR",
            Message = "Test message",
            Metodo = "TestMethod",
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
            new MessageLog { Tipo = "INFO", Message = "Info message", Metodo = "Method1", Path = "/path1" },
            new MessageLog { Tipo = "DEBUG", Message = "Debug message", Metodo = "Method2", Path = "/path2" },
            new MessageLog { Tipo = "WARNING", Message = "Warning message", Metodo = "Method3", Path = "/path3" },
            new MessageLog { Tipo = "ERROR", Message = "Error message", Metodo = "Method4", Path = "/path4" },
            new MessageLog { Tipo = "FATAL", Message = "Fatal message", Metodo = "Method5", Path = "/path5" }
        };

        // Assert
        foreach (var log in logTypes) {
            log.ToString().Should().NotBeNullOrWhiteSpace();
            log.Tipo.Should().NotBeNullOrEmpty();
            ((string)log.Message).Should().NotBeNull(); // Cast to string for FluentAssertions
            log.Metodo.Should().NotBeNullOrEmpty();
            log.Path.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void MessageLog_WithStringMessage_ShouldWorkCorrectly() {
        // Arrange
        var stringMessage = "Simple string message";

        // Act
        var messageLog = new MessageLog {
            Tipo = "INFO",
            Message = stringMessage,
            Metodo = "StringTest",
            Path = "/string/test"
        };

        // Assert
        ((string)messageLog.Message).Should().Be(stringMessage);
        messageLog.ToString().Should().Contain($"\"Message\": \"{stringMessage}\"");
    }

    [Fact]
    public void MessageLog_WithNumericMessage_ShouldWorkCorrectly() {
        // Arrange
        var numericMessage = 42;

        // Act
        var messageLog = new MessageLog {
            Tipo = "DEBUG",
            Message = numericMessage,
            Metodo = "NumericTest",
            Path = "/numeric/test"
        };

        // Assert
        ((int)messageLog.Message).Should().Be(numericMessage);
        messageLog.ToString().Should().Contain("\"Message\": 42");
    }
}
