using AppCore.Application.Wrappers;
using AppCore.Application.Serialization;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Wrappers;

public class PageResultTests {
    [Fact]
    public void PageResult_DefaultConstructor_ShouldInitializeProperties() {
        // Act
        var pageResult = new PageResult<string>();

        // Assert
        pageResult.Items.Should().NotBeNull().And.BeEmpty();
        pageResult.TotalPages.Should().Be(0);
        pageResult.CurrentPage.Should().Be(0);
        pageResult.Count.Should().Be(0);
    }

    [Fact]
    public void IsFirstPage_WhenCurrentPageIsOne_ShouldReturnTrue() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 1,
            TotalPages = 5
        };

        // Act & Assert
        pageResult.IsFirstPage.Should().BeTrue();
    }

    [Fact]
    public void IsFirstPage_WhenCurrentPageIsNotOne_ShouldReturnFalse() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 2,
            TotalPages = 5
        };

        // Act & Assert
        pageResult.IsFirstPage.Should().BeFalse();
    }

    [Fact]
    public void IsLastPage_WhenCurrentPageEqualsToTotalPages_ShouldReturnTrue() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 5,
            TotalPages = 5
        };

        // Act & Assert
        pageResult.IsLastPage.Should().BeTrue();
    }

    [Fact]
    public void IsLastPage_WhenCurrentPageIsNotLast_ShouldReturnFalse() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 3,
            TotalPages = 5
        };

        // Act & Assert
        pageResult.IsLastPage.Should().BeFalse();
    }

    [Fact]
    public void PageResult_WithItems_ShouldSetItemsCorrectly() {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var pageResult = new PageResult<string> {
            Items = items,
            CurrentPage = 1,
            TotalPages = 1,
            Count = 3
        };

        // Act & Assert
        pageResult.Items.Should().BeEquivalentTo(items);
        pageResult.Count.Should().Be(3);
    }

    [Fact]
    public void PageResult_WithSinglePage_ShouldBeBothFirstAndLastPage() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 1,
            TotalPages = 1
        };

        // Act & Assert
        pageResult.IsFirstPage.Should().BeTrue();
        pageResult.IsLastPage.Should().BeTrue();
    }

    [Fact]
    public void PageResult_WithEmptyPages_ShouldHandleEdgeCases() {
        // Arrange
        var pageResult = new PageResult<string> {
            CurrentPage = 0,
            TotalPages = 0
        };

        // Act & Assert
        pageResult.IsFirstPage.Should().BeFalse();
        pageResult.IsLastPage.Should().BeTrue(); // 0 == 0
    }

    [Fact]
    public void PageResult_WithComplexObject_ShouldWorkCorrectly() {
        // Arrange
        var complexItems = new List<JsonTestModel>
        {
            new() { Id = 1, Name = "Test1" },
            new() { Id = 2, Name = "Test2" }
        };

        var pageResult = new PageResult<JsonTestModel> {
            Items = complexItems,
            CurrentPage = 2,
            TotalPages = 3,
            Count = 2
        };

        // Act & Assert
        pageResult.Items.Should().HaveCount(2);
        pageResult.Items[0].Name.Should().Be("Test1");
        pageResult.IsFirstPage.Should().BeFalse();
        pageResult.IsLastPage.Should().BeFalse();
    }

    [Fact]
    public void PageResult_PropertySetters_ShouldWorkCorrectly() {
        // Arrange
        var pageResult = new PageResult<string>();

        // Act
        pageResult.Items = new List<string> { "Item1", "Item2", "Item3" };
        pageResult.CurrentPage = 2;
        pageResult.TotalPages = 4;
        pageResult.Count = 3;

        // Assert
        pageResult.Items.Should().Equal("Item1", "Item2", "Item3");
        pageResult.CurrentPage.Should().Be(2);
        pageResult.TotalPages.Should().Be(4);
        pageResult.Count.Should().Be(3);
    }
}
