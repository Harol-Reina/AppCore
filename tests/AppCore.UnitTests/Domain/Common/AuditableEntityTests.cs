using OrionSoft.AppCore.Domain.Common;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Domain.Common;

// Test implementation of AuditableEntity for testing purposes
public class TestAuditableEntity : AuditableEntity {
    public string? Name { get; set; }
}

public class AuditableEntityTests {
    [Fact]
    public void CreatedBy_ShouldBeSettable() {
        // Arrange
        var entity = new TestAuditableEntity();
        var createdBy = "user123";

        // Act
        entity.CreatedBy = createdBy;

        // Assert
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable() {
        // Arrange
        var entity = new TestAuditableEntity();
        var createdAt = DateTime.UtcNow;

        // Act
        entity.CreatedAt = createdAt;

        // Assert
        entity.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void UpdatedBy_ShouldBeSettable() {
        // Arrange
        var entity = new TestAuditableEntity();
        var updatedBy = "user456";

        // Act
        entity.UpdatedBy = updatedBy;

        // Assert
        entity.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable() {
        // Arrange
        var entity = new TestAuditableEntity();
        var updatedAt = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = updatedAt;

        // Assert
        entity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void DefaultValues_ShouldBeNull() {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.CreatedBy.Should().BeNull();
        entity.CreatedAt.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSetToNull() {
        // Arrange
        var entity = new TestAuditableEntity {
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "user2",
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        entity.CreatedBy = null;
        entity.CreatedAt = null;
        entity.UpdatedBy = null;
        entity.UpdatedAt = null;

        // Assert
        entity.CreatedBy.Should().BeNull();
        entity.CreatedAt.Should().BeNull();
        entity.UpdatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
    }
}
