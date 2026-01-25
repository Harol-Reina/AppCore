using System.Text.Json;
using AppCore.Infrastructure.Data.DAOs.Enum;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Data.DAOs.Enum;

public class QuotationStatusTests {
    [Fact]
    public void QuotationStatus_Started_ShouldHaveCorrectValue() {
        // Act
        var value = QuotationStatus.Started;

        // Assert
        value.Should().Be(QuotationStatus.Started);
        ((int)value).Should().Be(0);
    }

    [Fact]
    public void QuotationStatus_Updated_ShouldHaveCorrectValue() {
        // Act
        var value = QuotationStatus.Updated;

        // Assert
        value.Should().Be(QuotationStatus.Updated);
        ((int)value).Should().Be(1);
    }

    [Fact]
    public void QuotationStatus_Finished_ShouldHaveCorrectValue() {
        // Act
        var value = QuotationStatus.Finished;

        // Assert
        value.Should().Be(QuotationStatus.Finished);
        ((int)value).Should().Be(2);
    }

    [Fact]
    public void QuotationStatus_CoverageNote_ShouldHaveCorrectValue() {
        // Act
        var value = QuotationStatus.CoverageNote;

        // Assert
        value.Should().Be(QuotationStatus.CoverageNote);
        ((int)value).Should().Be(3);
    }

    [Fact]
    public void QuotationStatus_ShouldSerializeAsString() {
        // Arrange
        var status = QuotationStatus.Started;

        // Act
        var json = JsonSerializer.Serialize(status);

        // Assert
        json.Should().Be("\"Started\"");
    }

    [Fact]
    public void QuotationStatus_ShouldDeserializeFromString() {
        // Arrange
        var json = "\"Finished\"";

        // Act
        var status = JsonSerializer.Deserialize<QuotationStatus>(json);

        // Assert
        status.Should().Be(QuotationStatus.Finished);
    }

    [Fact]
    public void QuotationStatus_AllValues_ShouldBeDistinct() {
        // Arrange
        var allValues = System.Enum.GetValues<QuotationStatus>();

        // Assert
        allValues.Should().HaveCount(4);
        allValues.Should().OnlyHaveUniqueItems();
    }
}
