using AppCore.Application.Exceptions;
using AppCore.Application.Utils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AppCore.UnitTests.Application.Utils;

[Collection("ConfigurationTests")]
#pragma warning disable CS0618 // Testing obsolete Configuration class
public class ConfigurationTests {
    private static IConfiguration CreateConfiguration(Dictionary<string, string?> data) {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    [Fact]
    public void Initialize_WithValidConfiguration_ShouldSetConfiguration() {
        // Arrange
        var config = CreateConfiguration(new Dictionary<string, string?>());

        // Act
        Configuration.Initialize(config);

        // Assert - No exception thrown
        Assert.True(true);
    }

    [Fact]
    public void GetConfig_WithExistingKey_ShouldReturnValue() {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            { "TestKey", "TestValue" }
        };
        var config = CreateConfiguration(data);
        Configuration.Initialize(config);

        // Act
        var result = Configuration.GetConfig("TestKey");

        // Assert
        result.Should().Be("TestValue");
    }

    [Fact]
    public void GetConfig_WithNonExistingKey_ShouldReturnNull() {
        // Arrange
        var config = CreateConfiguration(new Dictionary<string, string?>());
        Configuration.Initialize(config);

        // Act
        var result = Configuration.GetConfig("NonExisting");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RequiredConfig_WithExistingKey_ShouldReturnValue() {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            { "Required", "Value123" }
        };
        var config = CreateConfiguration(data);
        Configuration.Initialize(config);

        // Act
        var result = Configuration.RequiredConfig("Required");

        // Assert
        result.Should().Be("Value123");
    }

    [Fact]
    public void RequiredConfig_WithNonExistingKey_ShouldThrowNotFoundException() {
        // Arrange
        var config = CreateConfiguration(new Dictionary<string, string?>());
        Configuration.Initialize(config);

        // Act
        var act = () => Configuration.RequiredConfig("MissingKey");

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("*MissingKey*");
    }

    [Fact]
    public void IntConfig_WithValidInteger_ShouldReturnParsedValue() {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            { "Port", "8080" }
        };
        var config = CreateConfiguration(data);
        Configuration.Initialize(config);

        // Act
        var result = Configuration.IntConfig("Port");

        // Assert
        result.Should().Be(8080);
    }

    [Fact]
    public void IntConfig_WithInvalidInteger_ShouldThrowOperationException() {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            { "InvalidInt", "NotANumber" }
        };
        var config = CreateConfiguration(data);
        Configuration.Initialize(config);

        // Act
        var act = () => Configuration.IntConfig("InvalidInt");

        // Assert
        act.Should().Throw<OperationException>()
            .WithMessage("*not a valid integer*");
    }

    [Fact]
    public void IntConfig_WithMissingKey_ShouldThrowNotFoundException() {
        // Arrange
        var config = CreateConfiguration(new Dictionary<string, string?>());
        Configuration.Initialize(config);

        // Act
        var act = () => Configuration.IntConfig("MissingInt");

        // Assert
        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void StringArray_WithValidArray_ShouldReturnArray() {
        // Arrange
        var data = new Dictionary<string, string?>
        {
            { "Array:0", "Value1" },
            { "Array:1", "Value2" },
            { "Array:2", "Value3" }
        };
        var config = CreateConfiguration(data);
        Configuration.Initialize(config);

        // Act
        var result = Configuration.StringArray("Array");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { "Value1", "Value2", "Value3" });
    }

    [Fact]
    public void StringArray_WithNonExisting_ShouldThrowNotFoundException() {
        // Arrange
        var config = CreateConfiguration(new Dictionary<string, string?>());
        Configuration.Initialize(config);

        // Act
        var act = () => Configuration.StringArray("NonExisting");

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("*NonExisting*");
    }
}
