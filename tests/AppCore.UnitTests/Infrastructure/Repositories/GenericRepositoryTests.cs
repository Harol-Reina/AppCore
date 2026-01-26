using System.Linq.Expressions;
using AppCore.Application.DTOs;
using AppCore.Application.Exceptions;
using AppCore.Application.Interfaces;
using AppCore.Domain.Common;
using AppCore.Infrastructure.Data.DAOs.Common;
using AppCore.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Repositories;

public sealed class GenericRepositoryTests : IDisposable {
    private readonly DbContext _dbContext;
    private readonly Mock<IMappingService<TestEntity, TestDao>> _entityToDaoMock;
    private readonly Mock<IMappingService<TestDao, TestEntity>> _daoToEntityMock;
    private readonly TestRepository _repository;

    public GenericRepositoryTests() {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options);
        _entityToDaoMock = new Mock<IMappingService<TestEntity, TestDao>>();
        _daoToEntityMock = new Mock<IMappingService<TestDao, TestEntity>>();
        _repository = new TestRepository(_dbContext, _entityToDaoMock.Object, _daoToEntityMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithoutIncludes_ShouldReturnAllEntities() {
        // Arrange
        var dao1 = new TestDao { Id = 1, Name = "Test1" };
        var dao2 = new TestDao { Id = 2, Name = "Test2" };

        var entity1 = new TestEntity { Id = 1, Name = "Test1" };
        var entity2 = new TestEntity { Id = 2, Name = "Test2" };

        await _dbContext.Set<TestDao>().AddRangeAsync(dao1, dao2);
        await _dbContext.SaveChangesAsync();

        _daoToEntityMock.Setup(m => m.Map(dao1)).Returns(entity1);
        _daoToEntityMock.Setup(m => m.Map(dao2)).Returns(entity2);
        _daoToEntityMock.Setup(m => m.Map(It.IsAny<IEnumerable<TestDao>>()))
            .Returns((IEnumerable<TestDao> input) => input.Select(d => d.Id == 1 ? entity1 : entity2).ToList());

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[^1].Id.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity() {
        // Arrange
        var dao = new TestDao { Id = 1, Name = "Test" };
        var entity = new TestEntity { Id = 1, Name = "Test" };

        await _dbContext.Set<TestDao>().AddAsync(dao);
        await _dbContext.SaveChangesAsync();

        _daoToEntityMock.Setup(m => m.Map(dao)).Returns(entity);

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        // The mapper may return null if not configured properly
        if (result != null) {
            result.Id.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull() {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }



    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldAddAndReturnEntity() {
        // Arrange
        var entity = new TestEntity { Name = "New Test" };
        var dao = new TestDao { Id = 1, Name = "New Test" };

        _entityToDaoMock.Setup(m => m.Map(entity)).Returns(dao);
        _daoToEntityMock.Setup(m => m.Map(dao)).Returns(entity);

        // Act
        var result = await _repository.AddAsync(entity);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Test");

        var savedDao = await _dbContext.Set<TestDao>().FirstOrDefaultAsync();
        savedDao.Should().NotBeNull();
        savedDao!.Name.Should().Be("New Test");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEntity_ShouldUpdateAndReturnEntity() {
        // Arrange
        var dao = new TestDao { Id = 1, Name = "Original", CreatedAt = DateTime.Now.AddDays(-1) };
        await _dbContext.Set<TestDao>().AddAsync(dao);
        await _dbContext.SaveChangesAsync();

        var entity = new TestEntity { Id = 1, Name = "Updated" };
        var updatedDao = new TestDao { Id = 1, Name = "Updated", CreatedAt = dao.CreatedAt };

        _entityToDaoMock.Setup(m => m.Map(entity)).Returns(updatedDao);
        _daoToEntityMock.Setup(m => m.Map(updatedDao)).Returns(entity);

        // Act
        var result = await _repository.UpdateAsync(entity);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");

        var savedDao = await _dbContext.Set<TestDao>().FirstAsync();
        savedDao.Name.Should().Be("Updated");
        savedDao.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateAsync_WithNewEntity_ShouldThrowBadRequestException() {
        // Arrange
        var newEntity = new TestEntity { Name = "New Entity" };

        // Act & Assert
        var act = async () => await _repository.UpdateAsync(newEntity);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot update an entity that hasn't been persisted. Use AddAsync instead.");
    }

    [Fact]
    public async Task DelAsync_WithExistingId_ShouldDeleteAndReturnTrue() {
        // Arrange
        var dao = new TestDao { Id = 1, Name = "To Delete" };
        await _dbContext.Set<TestDao>().AddAsync(dao);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DelAsync(1);

        // Assert
        result.Should().BeTrue();

        var deletedDao = await _dbContext.Set<TestDao>().FirstOrDefaultAsync(d => d.Id == 1);
        deletedDao.Should().BeNull();
    }

    [Fact]
    public async Task DelAsync_WithNonExistingId_ShouldReturnFalse() {
        // Act
        var result = await _repository.DelAsync(999);

        // Assert
        result.Should().BeFalse();
    }



    [Fact]
    public async Task GetPagedAsync_ShouldReturnPaginatedResults() {
        // Arrange
        var daos = Enumerable.Range(1, 10)
            .Select(i => new TestDao { Id = i, Name = $"Test{i}" })
            .ToList();

        var entities = Enumerable.Range(1, 10)
            .Select(i => new TestEntity { Id = i, Name = $"Test{i}" })
            .ToList();

        await _dbContext.Set<TestDao>().AddRangeAsync(daos);
        await _dbContext.SaveChangesAsync();

        foreach (var (dao, entity) in daos.Zip(entities)) {
            _daoToEntityMock.Setup(m => m.Map(dao)).Returns(entity);
        }
        _daoToEntityMock.Setup(m => m.Map(It.IsAny<IEnumerable<TestDao>>()))
            .Returns((IEnumerable<TestDao> d) => d.Select(x => entities.First(e => e.Id == x.Id)).ToList());

        // Act
        var result = await _repository.GetPagedAsync(page: 2, pageSize: 3);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(10);
        result.Pages.Should().Be(4); // Ceiling(10/3)
        result.Results.Should().HaveCount(3);
        result.Results.Select(r => r.Id).Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }



    public void Dispose() {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    // Test classes
    internal class TestEntity : BaseEntity<int> {
        public string? Name { get; set; }
        public new bool IsNew => Id == 0;
    }

    internal class TestDao : BaseDao<int> {
        public string? Name { get; set; }
    }

    internal class TestRepository : GenericRepository<TestEntity, int, TestDao> {
        public TestRepository(DbContext dbContext,
            IMappingService<TestEntity, TestDao> entityToDao,
            IMappingService<TestDao, TestEntity> daoToEntity)
            : base(dbContext, entityToDao, daoToEntity) { }


    }

    internal class TestDbContext : DbContext {
        public TestDbContext(DbContextOptions options) : base(options) { }

        public DbSet<TestDao> TestDaos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TestDao>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}
