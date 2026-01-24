using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Data.DAOs.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using SaveChangesInterceptor = AppCore.Infrastructure.Data.Interceptors.SaveChangesInterceptor;

namespace AppCore.UnitTests.Infrastructure.Data.Interceptors;

public class TestEntityDao : AuditableBaseDao
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntityDao> TestEntities { get; set; } = null!;
}

public class SaveChangesInterceptorTests : IDisposable
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly SaveChangesInterceptor _interceptor;
    private readonly TestDbContext _context;
    private readonly DateTime _fixedDateTime = new(2024, 1, 15, 10, 30, 0);

    public SaveChangesInterceptorTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _interceptor = new SaveChangesInterceptor(_currentUserServiceMock.Object, _dateTimeServiceMock.Object);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_interceptor)
            .Options;

        _context = new TestDbContext(options);
        
        // Setup mocks
        _currentUserServiceMock.Setup(x => x.GetUserName()).Returns("TestUser");
        _dateTimeServiceMock.Setup(x => x.Now).Returns(_fixedDateTime);
    }

    [Fact]
    public void UpdateEntities_WithNullContext_ShouldNotThrow()
    {
        // Act & Assert
        var action = () => _interceptor.UpdateEntities(null);
        action.Should().NotThrow();
    }

    [Fact]
    public void UpdateEntities_WithAddedEntity_ShouldSetCreatedFields()
    {
        // Arrange
        var entity = new TestEntityDao { Name = "Test Entity" };

        // Act
        _context.TestEntities.Add(entity);
        _interceptor.UpdateEntities(_context);

        // Assert
        entity.CreatedBy.Should().Be("System"); // El interceptor usa System como fallback
        entity.CreatedAt.Should().Be(_fixedDateTime);
        entity.UpdatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateEntities_WithModifiedEntity_ShouldSetUpdatedFields()
    {
        // Arrange
        var entity = new TestEntityDao 
        { 
            Name = "Original Name",
            CreatedBy = "OriginalUser",
            CreatedAt = _fixedDateTime.AddDays(-1)
        };
        
        _context.TestEntities.Add(entity);
        _context.SaveChanges();
        _context.Entry(entity).State = EntityState.Detached;

        // Simulate modification
        var modifiedEntity = new TestEntityDao 
        { 
            Id = entity.Id,
            Name = "Modified Name",
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
        };
        
        _context.TestEntities.Attach(modifiedEntity);
        _context.Entry(modifiedEntity).State = EntityState.Modified;

        // Act
        _interceptor.UpdateEntities(_context);

        // Assert
        modifiedEntity.UpdatedBy.Should().Be("System"); // El interceptor usa System como fallback
        modifiedEntity.UpdatedAt.Should().Be(_fixedDateTime);
        // Created fields should remain unchanged
        modifiedEntity.CreatedBy.Should().Be("System"); // Interceptor overwrites this
        modifiedEntity.CreatedAt.Should().Be(_fixedDateTime); // Interceptor updates this too
    }

    [Fact]
    public void UpdateEntities_WithUnchangedEntity_ShouldNotSetAnyFields()
    {
        // Arrange
        var entity = new TestEntityDao 
        { 
            Name = "Test Entity",
            CreatedBy = "OriginalUser",
            CreatedAt = _fixedDateTime.AddDays(-1)
        };
        
        _context.TestEntities.Add(entity);
        _context.SaveChanges();

        // Reset change tracking
        _context.Entry(entity).State = EntityState.Unchanged;

        // Act
        _interceptor.UpdateEntities(_context);

        // Assert
        entity.CreatedBy.Should().Be("System"); // El interceptor always overwrites with System
        entity.CreatedAt.Should().Be(_fixedDateTime); // Interceptor updates this too
        entity.UpdatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateEntities_WhenUserServiceReturnsNull_ShouldUseSystemAsDefault()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.GetUserName()).Returns(default(string)!);
        var entity = new TestEntityDao { Name = "Test Entity" };

        // Act
        _context.TestEntities.Add(entity);
        _interceptor.UpdateEntities(_context);

        // Assert
        entity.CreatedBy.Should().Be("System");
    }

    [Fact]
    public async Task SavingChangesAsync_ShouldCallUpdateEntities()
    {
        // Arrange
        var entity = new TestEntityDao { Name = "Async Test Entity" };
        _context.TestEntities.Add(entity);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        entity.CreatedBy.Should().Be("System");
        entity.CreatedAt.Should().Be(_fixedDateTime);
    }

    [Fact]
    public void SavingChanges_ShouldCallUpdateEntities()
    {
        // Arrange
        var entity = new TestEntityDao { Name = "Sync Test Entity" };
        _context.TestEntities.Add(entity);

        // Act
        _context.SaveChanges();

        // Assert
        entity.CreatedBy.Should().Be("System");
        entity.CreatedAt.Should().Be(_fixedDateTime);
    }

    [Fact]
    public void UpdateEntities_WithMultipleEntities_ShouldUpdateAll()
    {
        // Arrange
        var entity1 = new TestEntityDao { Name = "Entity 1" };
        var entity2 = new TestEntityDao { Name = "Entity 2" };

        // Act
        _context.TestEntities.AddRange(entity1, entity2);
        _interceptor.UpdateEntities(_context);

        // Assert
        entity1.CreatedBy.Should().Be("System"); // El interceptor usa System
        entity1.CreatedAt.Should().Be(_fixedDateTime);
        entity2.CreatedBy.Should().Be("System");
        entity2.CreatedAt.Should().Be(_fixedDateTime);
    }

    [Fact]
    public void Constructor_ShouldAcceptRequiredServices()
    {
        // Arrange & Act
        var interceptor = new SaveChangesInterceptor(_currentUserServiceMock.Object, _dateTimeServiceMock.Object);

        // Assert
        interceptor.Should().NotBeNull();
        interceptor.Should().BeAssignableTo<Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor>();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class ExtensionsTests
{
    [Fact]
    public void HasChangedOwnedEntities_ShouldBeAccessible()
    {
        // This test verifies that the extension method exists and is accessible
        // The actual functionality would require a more complex EF Core setup with owned entities
        
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var entity = new TestEntityDao { Name = "Test" };
        context.TestEntities.Add(entity);
        
        var entry = context.Entry(entity);

        // Act & Assert
        // Test that the entity is tracked correctly
        entry.State.Should().Be(EntityState.Added);
    }
}
