using AppCore.Infrastructure.Data.DAOs.Common;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Data.DAOs.Common;

internal class TestAuditableDao : AuditableBaseDao {
    public string Name { get; set; } = string.Empty;
}

public class AuditableBaseDaoTests {
    [Fact]
    public void AuditableBaseDao_CreatedAt_ShouldBeSetToNowByDefault() {
        // Arrange
        var beforeCreation = DateTime.Now.AddSeconds(-1);

        // Act
        var dao = new TestAuditableDao();
        var afterCreation = DateTime.Now.AddSeconds(1);

        // Assert
        dao.CreatedAt.Should().BeAfter(beforeCreation);
        dao.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public void AuditableBaseDao_CreatedBy_ShouldBeNullByDefault() {
        // Arrange & Act
        var dao = new TestAuditableDao();

        // Assert
        dao.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void AuditableBaseDao_UpdatedAt_ShouldBeNullByDefault() {
        // Arrange & Act
        var dao = new TestAuditableDao();

        // Assert
        dao.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void AuditableBaseDao_UpdatedBy_ShouldBeNullByDefault() {
        // Arrange & Act
        var dao = new TestAuditableDao();

        // Assert
        dao.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void AuditableBaseDao_PropertiesShouldBeSettable() {
        // Arrange
        var dao = new TestAuditableDao();
        var createdAt = DateTime.Now.AddDays(-1);
        var updatedAt = DateTime.Now;
        var createdBy = "TestUser";
        var updatedBy = "UpdateUser";

        // Act
        dao.CreatedAt = createdAt;
        dao.CreatedBy = createdBy;
        dao.UpdatedAt = updatedAt;
        dao.UpdatedBy = updatedBy;

        // Assert
        dao.CreatedAt.Should().Be(createdAt);
        dao.CreatedBy.Should().Be(createdBy);
        dao.UpdatedAt.Should().Be(updatedAt);
        dao.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void AuditableBaseDao_CanSetCreatedByToNull() {
        // Arrange
        var dao = new TestAuditableDao {
            CreatedBy = "InitialUser"
        };

        // Act
        dao.CreatedBy = null;

        // Assert
        dao.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void AuditableBaseDao_CanSetUpdatedByToNull() {
        // Arrange
        var dao = new TestAuditableDao {
            UpdatedBy = "UpdateUser"
        };

        // Act
        dao.UpdatedBy = null;

        // Assert
        dao.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void AuditableBaseDao_UpdatedAt_CanBeSetToNull() {
        // Arrange
        var dao = new TestAuditableDao {
            UpdatedAt = DateTime.Now
        };

        // Act
        dao.UpdatedAt = null;

        // Assert
        dao.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task AuditableBaseDao_WithMultipleInstances_ShouldHaveDifferentCreatedAt() {
        // Arrange & Act
        var dao1 = new TestAuditableDao();
        await Task.Delay(10); // Small delay to ensure different timestamps
        var dao2 = new TestAuditableDao();

        // Assert
        dao1.CreatedAt.Should().BeBefore(dao2.CreatedAt);
    }

    [Fact]
    public void AuditableBaseDao_InheritanceScenario_ShouldWorkCorrectly() {
        // Arrange
        var dao = new TestAuditableDao {
            Name = "Test Entity",
            CreatedBy = "System",
            UpdatedBy = "Admin"
        };

        // Act & Assert
        dao.Name.Should().Be("Test Entity");
        dao.CreatedBy.Should().Be("System");
        dao.UpdatedBy.Should().Be("Admin");
        dao.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}
