using AppCore;
using AppCore.Application.Behaviours;
using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Data.Interceptors;
using AppCore.Infrastructure.Services;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppCore.UnitTests;

public class DependencyInjectionTests {
    [Fact]
    public void AddCoreApplication_ShouldRegisterMediatRBehaviors() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCoreApplication();

        // Assert
        var behaviorRegistrations = services.Where(sd =>
            sd.ServiceType.IsGenericType &&
            sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        behaviorRegistrations.Should().HaveCount(2);
        behaviorRegistrations.Should().Contain(sd => sd.ImplementationType == typeof(UnhandledExceptionBehaviour<,>));
        behaviorRegistrations.Should().Contain(sd => sd.ImplementationType == typeof(ValidationBehaviour<,>));
    }

    [Fact]
    public void AddCoreApplication_ShouldRegisterSaveChangesInterceptor() {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();

        // Act
        services.AddCoreApplication();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var interceptor = serviceProvider.GetService<SaveChangesInterceptor>();
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void AddCoreApplication_ShouldRegisterDateTimeService() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCoreApplication();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dateTimeService = serviceProvider.GetService<IDateTimeService>();
        dateTimeService.Should().NotBeNull();
        dateTimeService.Should().BeOfType<DateTimeService>();
    }

    [Fact]
    public void AddCoreApplication_ShouldRegisterHttpContextAccessor() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCoreApplication();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        httpContextAccessor.Should().NotBeNull();
    }

    [Fact]
    public void AddCoreApplication_ShouldRegisterCurrentUserService() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCoreApplication();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var currentUserService = serviceProvider.GetService<ICurrentUserService>();
        currentUserService.Should().NotBeNull();
        currentUserService.Should().BeOfType<CurrentUserService>();
    }

    [Fact]
    public void AddCoreApplication_ShouldReturnServiceCollection() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCoreApplication();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCoreApplication_CurrentUserService_ShouldBeScoped() {
        // Arrange
        var services = new ServiceCollection();
        services.AddCoreApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddCoreApplication_DateTimeService_ShouldBeTransient() {
        // Arrange
        var services = new ServiceCollection();
        services.AddCoreApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDateTimeService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddCoreApplication_SaveChangesInterceptor_ShouldBeScoped() {
        // Arrange
        var services = new ServiceCollection();
        services.AddCoreApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SaveChangesInterceptor));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}
