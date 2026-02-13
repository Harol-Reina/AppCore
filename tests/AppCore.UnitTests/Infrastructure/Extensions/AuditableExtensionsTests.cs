using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Data.DAOs.Common;
using AppCore.Infrastructure.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Extensions;

internal class TestAuditableDao : IAuditableBaseDao {
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class AuditableExtensionsTests {
    private readonly Mock<ICurrentUserService> _userServiceMock;
    private const string TestUserName = "testuser";

    public AuditableExtensionsTests() {
        _userServiceMock = new Mock<ICurrentUserService>();
#pragma warning disable S3236 // Caller info arguments are required for Moq Setup matching
        _userServiceMock
            .Setup(x => x.GetUserName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(TestUserName);
#pragma warning restore S3236
    }

    [Fact]
    public void SetAuditCreate_ShouldSetCreatedAtToUtcNow() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditCreate(_userServiceMock.Object);

        // Assert
        dao.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetAuditCreate_ShouldSetCreatedByFromUserService() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditCreate(_userServiceMock.Object);

        // Assert
        dao.CreatedBy.Should().Be(TestUserName);
    }

    [Fact]
    public void SetAuditCreate_ShouldClearUpdateFields() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditCreate(_userServiceMock.Object);

        // Assert
        dao.UpdatedAt.Should().BeNull();
        dao.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void SetAuditCreate_ShouldClearExistingUpdateFields() {
        // Arrange
        var dao = new TestAuditableDao {
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "previous-user"
        };

        // Act
        dao.SetAuditCreate(_userServiceMock.Object);

        // Assert
        dao.UpdatedAt.Should().BeNull();
        dao.UpdatedBy.Should().BeNull();
    }

    [Fact]
    public void SetAuditUpdate_ShouldSetUpdatedAtToUtcNow() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditUpdate(_userServiceMock.Object);

        // Assert
        dao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetAuditUpdate_ShouldSetUpdatedByFromUserService() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditUpdate(_userServiceMock.Object);

        // Assert
        dao.UpdatedBy.Should().Be(TestUserName);
    }

    [Fact]
    public void SetAuditUpdate_ShouldNotModifyCreateFields() {
        // Arrange
        var originalCreatedAt = DateTime.UtcNow.AddDays(-5);
        const string originalCreatedBy = "original-creator";
        var dao = new TestAuditableDao {
            CreatedAt = originalCreatedAt,
            CreatedBy = originalCreatedBy
        };

        // Act
        dao.SetAuditUpdate(_userServiceMock.Object);

        // Assert
        dao.CreatedAt.Should().Be(originalCreatedAt);
        dao.CreatedBy.Should().Be(originalCreatedBy);
    }

    [Fact]
    public void SetAuditCreate_ThenSetAuditUpdate_ShouldPreserveCreateAndSetUpdateFields() {
        // Arrange
        var dao = new TestAuditableDao();

        // Act
        dao.SetAuditCreate(_userServiceMock.Object);
        var createdAt = dao.CreatedAt;
        var createdBy = dao.CreatedBy;

        dao.SetAuditUpdate(_userServiceMock.Object);

        // Assert
        dao.CreatedAt.Should().Be(createdAt);
        dao.CreatedBy.Should().Be(createdBy);
        dao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dao.UpdatedBy.Should().Be(TestUserName);
    }
}
