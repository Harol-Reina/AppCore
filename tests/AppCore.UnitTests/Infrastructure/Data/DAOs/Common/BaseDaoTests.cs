using AppCore.Infrastructure.Data.DAOs.Common;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Data.DAOs.Common;

internal class TestDao : IBaseDao<int> {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

internal class TestDaoGuid : IBaseDao<Guid> {
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

internal class TestDaoString : IBaseDao<string> {
    public string? Id { get; set; }
    public int Value { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class BaseDaoTests {
    [Fact]
    public void IsNew_WithDefaultIntId_ShouldReturnTrue() {
        // Arrange
        IBaseDao<int> dao = new TestDao { Id = 0 }; // Default int value is 0

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithValidIntId_ShouldReturnFalse() {
        // Arrange
        IBaseDao<int> dao = new TestDao { Id = 123 };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithDefaultGuidId_ShouldReturnTrue() {
        // Arrange
        IBaseDao<Guid> dao = new TestDaoGuid { Id = Guid.Empty }; // Default Guid value

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithValidGuidId_ShouldReturnFalse() {
        // Arrange
        IBaseDao<Guid> dao = new TestDaoGuid { Id = Guid.NewGuid() };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithNullStringId_ShouldReturnTrue() {
        // Arrange
        IBaseDao<string> dao = new TestDaoString { Id = null };

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithEmptyStringId_ShouldReturnFalse() {
        // Arrange
        IBaseDao<string> dao = new TestDaoString { Id = "" }; // Empty string is not default for string type (if nullable)

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithValidStringId_ShouldReturnFalse() {
        // Arrange
        IBaseDao<string> dao = new TestDaoString { Id = "test-id" };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void TestDao_ShouldImplementIBaseDao() {
        // Arrange & Act
        var dao = new TestDao();

        // Assert
        dao.Should().BeAssignableTo<IBaseDao<int>>();
        dao.Should().BeAssignableTo<IAuditableBaseDao>();
    }

    [Fact]
    public void Properties_ShouldBeSettable() {
        // Arrange
        var dao = new TestDao();
        var now = DateTime.Now;

        // Act
        dao.Id = 456;
        dao.Name = "Test Name";
        dao.CreatedAt = now;
        dao.CreatedBy = "TestUser";

        // Assert
        dao.Id.Should().Be(456);
        dao.Name.Should().Be("Test Name");
        dao.CreatedAt.Should().Be(now);
        dao.CreatedBy.Should().Be("TestUser");

        // IsNew check via interface
        ((IBaseDao<int>)dao).IsNew.Should().BeFalse();
    }
}
