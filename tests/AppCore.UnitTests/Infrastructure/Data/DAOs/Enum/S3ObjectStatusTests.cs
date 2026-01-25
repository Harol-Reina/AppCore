using AppCore.Infrastructure.Data.DAOs.Enum;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Data.DAOs.Enum;

public class S3ObjectStatusTests {
    [Fact]
    public void S3ObjectStatus_Saved_ShouldHaveCorrectValue() {
        // Act
        var value = S3ObjectStatus.Saved;

        // Assert
        value.Should().Be(S3ObjectStatus.Saved);
        ((int)value).Should().Be(0);
    }

    [Fact]
    public void S3ObjectStatus_Updated_ShouldHaveCorrectValue() {
        // Act
        var value = S3ObjectStatus.Updated;

        // Assert
        value.Should().Be(S3ObjectStatus.Updated);
        ((int)value).Should().Be(1);
    }

    [Fact]
    public void S3ObjectStatus_Equal_ShouldHaveCorrectValue() {
        // Act
        var value = S3ObjectStatus.Equal;

        // Assert
        value.Should().Be(S3ObjectStatus.Equal);
        ((int)value).Should().Be(2);
    }

    [Fact]
    public void S3ObjectStatus_Delete_ShouldHaveCorrectValue() {
        // Act
        var value = S3ObjectStatus.Delete;

        // Assert
        value.Should().Be(S3ObjectStatus.Delete);
        ((int)value).Should().Be(3);
    }

    [Fact]
    public void S3ObjectStatus_AllValues_ShouldBeDistinct() {
        // Arrange
        var allValues = System.Enum.GetValues<S3ObjectStatus>();

        // Assert
        allValues.Should().HaveCount(4);
        allValues.Should().OnlyHaveUniqueItems();
    }
}
