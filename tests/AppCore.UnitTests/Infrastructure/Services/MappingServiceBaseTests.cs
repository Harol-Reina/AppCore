using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Infrastructure.Services;

public class MappingServiceBaseTests {
    private class SourceModel {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class DestinationModel {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestMappingService : MappingServiceBase<SourceModel, DestinationModel> {
        protected override DestinationModel MapInternal(SourceModel source) {
            return new DestinationModel {
                Id = source.Id,
                Name = source.Name
            };
        }
    }

    private class ThrowingMappingService : MappingServiceBase<SourceModel, DestinationModel> {
        protected override DestinationModel MapInternal(SourceModel source) {
            throw new InvalidOperationException("Mapping failed");
        }
    }

    [Fact]
    public void Map_WithValidSource_ShouldReturnMappedDestination() {
        // Arrange
        var service = new TestMappingService();
        var source = new SourceModel { Id = 1, Name = "Test" };

        // Act
        var result = service.Map(source);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void Map_WithNullSource_ShouldThrowArgumentNullException() {
        // Arrange
        var service = new TestMappingService();

        // Act
        var act = () => service.Map((SourceModel)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_WhenMappingFails_ShouldThrowMappingException() {
        // Arrange
        var service = new ThrowingMappingService();
        var source = new SourceModel { Id = 1, Name = "Test" };

        // Act
        var act = () => service.Map(source);

        // Assert
        act.Should().Throw<MappingException>()
            .WithMessage("*Failed to map from SourceModel to DestinationModel*");
    }

    [Fact]
    public void Map_Collection_WithValidSources_ShouldReturnMappedDestinations() {
        // Arrange
        var service = new TestMappingService();
        var sources = new List<SourceModel>
        {
            new() { Id = 1, Name = "First" },
            new() { Id = 2, Name = "Second" },
            new() { Id = 3, Name = "Third" }
        };

        // Act
        var results = service.Map(sources).ToList();

        // Assert
        results.Should().HaveCount(3);
        results[0].Name.Should().Be("First");
        results[1].Name.Should().Be("Second");
        results[2].Name.Should().Be("Third");
    }

    [Fact]
    public void Map_Collection_WithNullCollection_ShouldThrowArgumentNullException() {
        // Arrange
        var service = new TestMappingService();

        // Act
        var act = () => service.Map((IEnumerable<SourceModel>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_Collection_WithEmptyCollection_ShouldReturnEmptyCollection() {
        // Arrange
        var service = new TestMappingService();
        var sources = new List<SourceModel>();

        // Act
        var results = service.Map(sources).ToList();

        // Assert
        results.Should().BeEmpty();
    }
}
