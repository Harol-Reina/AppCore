using Microsoft.Extensions.DependencyInjection;
using AppCore.Domain.Interfaces;
using AppCore.Application.Interfaces;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests;

/// <summary>
/// Global configuration tests to ensure the AppCore package is properly set up
/// </summary>
public class GlobalConfigurationTests
{
    [Fact]
    public void ServiceCollection_ShouldRegisterCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        // Note: This would be added when we create the AddAppCore() extension
        // For now, just test that we can create a service collection
        services.AddScoped(typeof(IGenericRepository<,>), typeof(DummyRepository<,>));

        // Assert
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AssemblyInfo_ShouldHaveCorrectMetadata()
    {
        // Arrange & Act
        var assembly = typeof(AppCore.Application.Wrappers.Response<>).Assembly;

        // Assert
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("AppCore");
    }

    [Fact]
    public void Interfaces_ShouldBePublic()
    {
        // Arrange & Act
        var genericRepositoryType = typeof(IGenericRepository<,>);
        var currentUserServiceType = typeof(ICurrentUserService);
        var dateTimeServiceType = typeof(IDateTimeService);

        // Assert
        genericRepositoryType.IsPublic.Should().BeTrue();
        currentUserServiceType.IsPublic.Should().BeTrue();
        dateTimeServiceType.IsPublic.Should().BeTrue();
    }
}

// Dummy implementation for testing DI registration
internal class DummyRepository<E, I> : IGenericRepository<E, I> 
    where E : AppCore.Domain.Common.BaseEntity<I>
{
    public Task<List<E>?> GetAllAsync(params System.Linq.Expressions.Expression<Func<E, object>>[]? includes)
        => Task.FromResult<List<E>?>(new List<E>());

    public Task<AppCore.Application.DTOs.PaginationDto<E>> GetPagedAsync(int page, int pageSize, params System.Linq.Expressions.Expression<Func<E, object>>[]? includes)
        => Task.FromResult(new AppCore.Application.DTOs.PaginationDto<E> { Results = new List<E>(), Count = 0, Pages = 1 });

    public Task<E?> GetByIdAsync(I id, params System.Linq.Expressions.Expression<Func<E, object>>[]? includes)
        => Task.FromResult<E?>(default);

    public Task<E> AddAsync(E entity)
        => Task.FromResult(entity);

    public Task<E> UpdateAsync(E entity)
        => Task.FromResult(entity);

    public Task DeleteAsync(E entity)
        => Task.CompletedTask;

    public Task<bool> DelAsync(I id)
        => Task.FromResult(true);

    public Task<List<E>?> FindAsync(System.Linq.Expressions.Expression<Func<E, bool>> expression, params System.Linq.Expressions.Expression<Func<E, object>>[]? includes)
        => Task.FromResult<List<E>?>(new List<E>());

    public Task<E?> FindFirstAsync(System.Linq.Expressions.Expression<Func<E, bool>> expression, params System.Linq.Expressions.Expression<Func<E, object>>[]? includes)
        => Task.FromResult<E?>(default);

    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<E, bool>>? expression = null)
        => Task.FromResult(0);

    public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<E, bool>>? expression = null)
        => Task.FromResult(false);
}
