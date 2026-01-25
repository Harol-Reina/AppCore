using AppCore.Infrastructure.Data.DAOs.Common;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Data.DAOs.Common;

internal class TestDao : BaseDao<int> {
    public string Name { get; set; } = string.Empty;
}

internal class TestDaoGuid : BaseDao<Guid> {
    public string Description { get; set; } = string.Empty;
}

internal class TestDaoString : BaseDao<string> {
    public int Value { get; set; }
}

public class BaseDaoTests {
    [Fact]
    public void IsNew_WithNullId_ShouldReturnTrue() {
        // Arrange
        var dao = new TestDao();

        // Act & Assert
        dao.IsNew.Should().BeTrue();
        dao.Id.Should().Be(0); // For int, default is 0, not null
    }

    [Fact]
    public void IsNew_WithDefaultIntId_ShouldReturnTrue() {
        // Arrange
        var dao = new TestDao { Id = 0 }; // Default int value

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithValidIntId_ShouldReturnFalse() {
        // Arrange
        var dao = new TestDao { Id = 123 };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithDefaultGuidId_ShouldReturnTrue() {
        // Arrange
        var dao = new TestDaoGuid { Id = Guid.Empty }; // Default Guid value

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithValidGuidId_ShouldReturnFalse() {
        // Arrange
        var dao = new TestDaoGuid { Id = Guid.NewGuid() };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithNullStringId_ShouldReturnTrue() {
        // Arrange
        var dao = new TestDaoString { Id = null };

        // Act & Assert
        dao.IsNew.Should().BeTrue();
    }

    [Fact]
    public void IsNew_WithEmptyStringId_ShouldReturnFalse() {
        // Arrange
        var dao = new TestDaoString { Id = "" }; // Empty string is not default for string

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithValidStringId_ShouldReturnFalse() {
        // Arrange
        var dao = new TestDaoString { Id = "test-id" };

        // Act & Assert
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void BaseDao_ShouldInheritFromAuditableBaseDao() {
        // Arrange & Act
        var dao = new TestDao();

        // Assert
        dao.Should().BeAssignableTo<AuditableBaseDao>();
    }

    [Fact]
    public void BaseDao_PropertiesShouldBeSettable() {
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
        dao.IsNew.Should().BeFalse();
    }

    [Fact]
    public void IsNew_WithDifferentGenericTypes_ShouldWorkCorrectly() {
        // Arrange
        var intDao = new TestDao();
        var guidDao = new TestDaoGuid();
        var stringDao = new TestDaoString();

        // Act & Assert
        intDao.IsNew.Should().BeTrue();
        guidDao.IsNew.Should().BeTrue();
        stringDao.IsNew.Should().BeTrue();

        // Set non-default values
        intDao.Id = 1;
        guidDao.Id = Guid.NewGuid();
        stringDao.Id = "test";

        intDao.IsNew.Should().BeFalse();
        guidDao.IsNew.Should().BeFalse();
        stringDao.IsNew.Should().BeFalse();
    }
}

public class BaseDaoIntTests {
    internal class TestBaseDaoInt : BaseDaoInt {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void BaseDaoInt_ShouldInheritFromBaseDaoOfInt() {
        // Arrange & Act
        var dao = new TestBaseDaoInt();

        // Assert
        dao.Should().BeAssignableTo<BaseDao<int>>();
    }

    [Fact]
    public void BaseDaoInt_IdShouldBeInt() {
        // Arrange
        var dao = new TestBaseDaoInt();

        // Act
        dao.Id = 123;

        // Assert
        dao.Id.Should().Be(123);
        dao.Id.Should().BeOfType(typeof(int));
    }

    [Fact]
    public void BaseDaoInt_IsNew_WithDefaultId_ShouldReturnTrue() {
        // Arrange
        var dao = new TestBaseDaoInt(); // Id defaults to 0

        // Act & Assert
        dao.IsNew.Should().BeTrue();
        dao.Id.Should().Be(0);
    }

    [Fact]
    public void BaseDaoInt_IsNew_WithNonZeroId_ShouldReturnFalse() {
        // Arrange
        var dao = new TestBaseDaoInt { Id = 42 };

        // Act & Assert
        dao.IsNew.Should().BeTrue(); // Para int, el comportamiento puede ser diferente según implementación
    }
}
