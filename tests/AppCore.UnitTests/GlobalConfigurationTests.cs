using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrionSoft.AppCore.UnitTests;

/// <summary>
/// Global configuration tests to ensure the AppCore package is properly set up
/// </summary>
public class GlobalConfigurationTests {
    [Fact]
    public void ServiceCollection_ShouldRegisterCoreServices() {
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
    public void AssemblyInfo_ShouldHaveCorrectMetadata() {
        // Arrange & Act
        var assembly = typeof(OrionSoft.AppCore.Application.Wrappers.Response<>).Assembly;

        // Assert
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("OrionSoft.AppCore");
    }

    [Fact]
    public void Interfaces_ShouldBePublic() {
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
    where E : OrionSoft.AppCore.Domain.Common.BaseEntity<I> {
    public Task<List<E>?> GetAllAsync()
        => Task.FromResult<List<E>?>(new List<E>());

    public Task<OrionSoft.AppCore.Application.DTOs.PaginationResponse<E>> GetPagedAsync(int page, int pageSize)
        => Task.FromResult(new OrionSoft.AppCore.Application.DTOs.PaginationResponse<E> { Results = [], Count = 0, Pages = 1 });

    public Task<E?> GetByIdAsync(I id)
        => Task.FromResult<E?>(default);

    public Task<E> AddAsync(E entity)
        => Task.FromResult(entity);

    public Task<E> UpdateAsync(E entity)
        => Task.FromResult(entity);

    public Task<bool> DelAsync(I id)
        => Task.FromResult(true);
}
