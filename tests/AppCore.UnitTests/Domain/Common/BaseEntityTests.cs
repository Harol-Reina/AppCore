using OrionSoft.AppCore.Domain.Common;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Domain.Common;

// Test implementations of BaseEntity for testing purposes
public class TestStringEntity : BaseEntity<string> {
    public TestStringEntity() : base() { }
    public TestStringEntity(string id) : base(id) { }
    public string? Name { get; set; }
}

public class TestIntEntity : BaseEntity<int> {
    public TestIntEntity() : base() { }
    public TestIntEntity(int id) : base(id) { }
    public string? Name { get; set; }
}

public class TestGuidEntity : BaseEntity<Guid> {
    public TestGuidEntity() : base() { }
    public TestGuidEntity(Guid id) : base(id) { }
    public string? Name { get; set; }
}

public class BaseEntityTests {
    [Fact]
    public void Constructor_WithoutId_ShouldCreateEntityWithNullId() {
        // Act
        var entity = new TestStringEntity();

        // Assert
        entity.Id.Should().BeNull();
        entity.IsNew.Should().BeTrue();
        entity.IsPersisted.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithId_ShouldCreateEntityWithId() {
        // Arrange
        var id = "test-id-123";

        // Act
        var entity = new TestStringEntity(id);

        // Assert
        entity.Id.Should().Be(id);
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithNullId_ShouldReturnTrue() {
        // Arrange
        var entity = new TestStringEntity();

        // Act & Assert
        entity.IsNew.Should().BeTrue();
        entity.IsPersisted.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithValidId_ShouldReturnFalse() {
        // Arrange
        var entity = new TestStringEntity("valid-id");

        // Act & Assert
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithDefaultIntId_ShouldReturnTrue() {
        // Arrange
        var entity = new TestIntEntity();
        entity.Id = 0; // Default int value

        // Act & Assert
        entity.IsNew.Should().BeTrue();
        entity.IsPersisted.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithNonDefaultIntId_ShouldReturnFalse() {
        // Arrange
        var entity = new TestIntEntity(42);

        // Act & Assert
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithDefaultGuidId_ShouldReturnTrue() {
        // Arrange
        var entity = new TestGuidEntity();
        entity.Id = Guid.Empty; // Default Guid value

        // Act & Assert
        entity.IsNew.Should().BeTrue();
        entity.IsPersisted.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithValidGuidId_ShouldReturnFalse() {
        // Arrange
        var validGuid = Guid.NewGuid();
        var entity = new TestGuidEntity(validGuid);

        // Act & Assert
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void IsPersisted_ShouldBeOppositeOfIsNew() {
        // Arrange
        var newEntity = new TestStringEntity();
        var persistedEntity = new TestStringEntity("existing-id");

        // Act & Assert
        newEntity.IsNew.Should().BeTrue();
        newEntity.IsPersisted.Should().BeFalse();

        persistedEntity.IsNew.Should().BeFalse();
        persistedEntity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void Id_ShouldBeSettable() {
        // Arrange
        var entity = new TestStringEntity();
        var newId = "new-id";

        // Act
        entity.Id = newId;

        // Assert
        entity.Id.Should().Be(newId);
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }

    [Fact]
    public void Entity_ShouldInheritFromAuditableEntity() {
        // Arrange & Act
        var entity = new TestStringEntity();

        // Assert
        entity.Should().BeAssignableTo<AuditableEntity>();
    }

    [Theory]
    [InlineData("")]   // Empty string is not null, so should be considered not new
    public void IsNew_WithEmptyStringId_ShouldReturnFalse(string emptyId) {
        // Arrange
        var entity = new TestStringEntity(emptyId);

        // Act & Assert
        entity.IsNew.Should().BeFalse();
        entity.IsPersisted.Should().BeTrue();
    }
}
