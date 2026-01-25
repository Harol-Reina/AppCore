using AppCore.Domain.Common;
using AppCore.Domain.Interfaces;
using AppCore.Infrastructure.Repositories;
using AppCore.Application.Interfaces;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TechTalk.SpecFlow;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace AppCore.SpecFlow.StepDefinitions;

[Binding]
public class GenericRepositoryStepDefinitions
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFixture _fixture;
    private TestEntity? _testEntity;
    private TestEntity? _retrievedEntity;
    private Exception? _thrownException;
    private IGenericRepository<TestEntity, int>? _repository;

    public GenericRepositoryStepDefinitions()
    {
        _fixture = new Fixture();
        
        // Configure test services
        var services = new ServiceCollection();
        ConfigureTestServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Given(@"I have a configured AppCore context")]
    public void GivenIHaveAConfiguredAppCoreContext()
    {
        _repository = _serviceProvider.GetRequiredService<IGenericRepository<TestEntity, int>>();
        _repository.Should().NotBeNull();
    }

    [Given(@"I have a test entity type ""(.*)""")]
    public void GivenIHaveATestEntityType(string entityType)
    {
        entityType.Should().Be("TestEntity");
    }

    [Given(@"I have a new entity with valid data")]
    public void GivenIHaveANewEntityWithValidData()
    {
        _testEntity = _fixture.Build<TestEntity>()
            .Without(x => x.Id) // New entity shouldn't have ID
            .Create();
        
        _testEntity.Should().NotBeNull();
        _testEntity.Id.Should().Be(0); // Default for int
    }

    [When(@"I call AddAsync on the repository")]
    public async Task WhenICallAddAsyncOnTheRepository()
    {
        try
        {
            _retrievedEntity = await _repository!.AddAsync(_testEntity!);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [Then(@"the entity should be saved successfully")]
    public void ThenTheEntityShouldBeSavedSuccessfully()
    {
        _thrownException.Should().BeNull();
        _retrievedEntity.Should().NotBeNull();
    }

    [Then(@"the entity should have an assigned ID")]
    public void ThenTheEntityShouldHaveAnAssignedId()
    {
        _retrievedEntity!.Id.Should().BeGreaterThan(0);
    }

    [Then(@"the entity should have audit fields populated")]
    public void ThenTheEntityShouldHaveAuditFieldsPopulated()
    {
        _retrievedEntity!.CreatedAt.Should().NotBeNull();
        _retrievedEntity.CreatedBy.Should().NotBeNullOrEmpty();
    }

    [Given(@"I have an existing entity in the database")]
    public async Task GivenIHaveAnExistingEntityInTheDatabase()
    {
        _testEntity = _fixture.Build<TestEntity>()
            .Without(x => x.Id)
            .Create();
        
        _testEntity = await _repository!.AddAsync(_testEntity);
        _testEntity.Should().NotBeNull();
        _testEntity.Id.Should().BeGreaterThan(0);
    }

    [When(@"I modify the entity data")]
    public void WhenIModifyTheEntityData()
    {
        _testEntity!.Name = "Modified Name";
        _testEntity.Description = "Modified Description";
    }

    [When(@"I call UpdateAsync on the repository")]
    public async Task WhenICallUpdateAsyncOnTheRepository()
    {
        try
        {
            _retrievedEntity = await _repository!.UpdateAsync(_testEntity!);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [Then(@"the entity should be updated successfully")]
    public void ThenTheEntityShouldBeUpdatedSuccessfully()
    {
        _thrownException.Should().BeNull();
        _retrievedEntity.Should().NotBeNull();
        _retrievedEntity!.Name.Should().Be(_testEntity!.Name);
        _retrievedEntity.Description.Should().Be(_testEntity.Description);
    }

    [Then(@"the UpdatedAt field should be current")]
    public void ThenTheUpdatedAtFieldShouldBeCurrent()
    {
        _retrievedEntity!.UpdatedAt.Should().NotBeNull();
        _retrievedEntity.UpdatedAt!.Value.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Then(@"the UpdatedBy field should be populated")]
    public void ThenTheUpdatedByFieldShouldBePopulated()
    {
        _retrievedEntity!.UpdatedBy.Should().NotBeNullOrEmpty();
        // The UpdatedBy field should be populated with some valid value
        // The exact value may depend on the current user service implementation
    }

    [When(@"I call GetByIdAsync with the entity ID")]
    public async Task WhenICallGetByIdAsyncWithTheEntityID()
    {
        try
        {
            _retrievedEntity = await _repository!.GetByIdAsync(_testEntity!.Id);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [Then(@"I should receive the correct entity")]
    public void ThenIShouldReceiveTheCorrectEntity()
    {
        _thrownException.Should().BeNull();
        _retrievedEntity.Should().NotBeNull();
        _retrievedEntity!.Id.Should().Be(_testEntity!.Id);
    }

    [Then(@"the entity data should match the stored data")]
    public void ThenTheEntityDataShouldMatchTheStoredData()
    {
        _retrievedEntity!.Name.Should().Be(_testEntity!.Name);
        _retrievedEntity.Description.Should().Be(_testEntity.Description);
        _retrievedEntity.CreatedAt.Should().Be(_testEntity.CreatedAt);
        _retrievedEntity.CreatedBy.Should().Be(_testEntity.CreatedBy);
    }

    [When(@"I call DelAsync with the entity ID")]
    public async Task WhenICallDelAsyncWithTheEntityID()
    {
        try
        {
            await _repository!.DelAsync(_testEntity!.Id);
        }
        catch (Exception ex)
        {
            _thrownException = ex;
        }
    }

    [Then(@"the entity should be removed from the database")]
    public async Task ThenTheEntityShouldBeRemovedFromTheDatabase()
    {
        _thrownException.Should().BeNull();
        
        // Verify the entity was deleted by trying to retrieve it
        var deletedEntity = await _repository!.GetByIdAsync(_testEntity!.Id);
        deletedEntity.Should().BeNull();
    }

    [Then(@"subsequent GetByIdAsync should return null")]
    public async Task ThenSubsequentGetByIdAsyncShouldReturnNull()
    {
        var result = await _repository!.GetByIdAsync(_testEntity!.Id);
        result.Should().BeNull();
    }

    private static void ConfigureTestServices(IServiceCollection services)
    {
        // Configure in-memory database
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Register mapping service
        services.AddScoped<IMappingService<TestEntity, TestEntityDao>, TestEntityMappingService>();
        services.AddScoped<IMappingService<TestEntityDao, TestEntity>, TestEntityReverseMappingService>();

        // Register AppCore services
        services.AddScoped<IGenericRepository<TestEntity, int>, TestEntityRepository>();
        
        // Mock external dependencies
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.GetUserName()).Returns("TestUser");
        services.AddSingleton(currentUserService.Object);

        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(x => x.Now).Returns(DateTime.Now);
        services.AddSingleton(dateTimeService.Object);
    }
}

// Test classes
internal class TestEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

internal class TestEntityDao : BaseDao<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

internal class TestEntityRepository : GenericRepository<TestEntity, int, TestEntityDao>
{
    public TestEntityRepository(TestDbContext context, 
        IMappingService<TestEntity, TestEntityDao> entityToDao,
        IMappingService<TestEntityDao, TestEntity> daoToEntity) 
        : base(context, entityToDao, daoToEntity) { }
}

internal class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public DbSet<TestEntityDao> TestEntities { get; set; } = null!;
}

internal class TestEntityMappingService : IMappingService<TestEntity, TestEntityDao>
{
    public TestEntityDao Map(TestEntity source)
    {
        return new TestEntityDao
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description
        };
    }

    public IEnumerable<TestEntityDao> Map(IEnumerable<TestEntity> sources)
    {
        return sources.Select(Map);
    }
}

internal class TestEntityReverseMappingService : IMappingService<TestEntityDao, TestEntity>
{
    public TestEntity Map(TestEntityDao source)
    {
        return new TestEntity
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description
        };
    }

    public IEnumerable<TestEntity> Map(IEnumerable<TestEntityDao> sources)
    {
        return sources.Select(Map);
    }
}
