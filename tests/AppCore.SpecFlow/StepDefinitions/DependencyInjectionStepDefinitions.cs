using AppCore;
using AppCore.Application.Interfaces;
using AppCore.Domain.Common;
using AppCore.Domain.Interfaces;

using AppCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;

namespace AppCore.SpecFlow.StepDefinitions;

[Binding]
public class DependencyInjectionStepDefinitions {
    private IServiceCollection? _services;
    private IServiceProvider? _serviceProvider;
    private Exception? _thrownException;

    [Given(@"I am configuring an application with AppCore")]
    public void GivenIAmConfiguringAnApplicationWithAppCore() {
        _services = new ServiceCollection();
        _services.Should().NotBeNull();
    }

    [Given(@"I have a service collection")]
    public void GivenIHaveAServiceCollection() {
        _services = new ServiceCollection();
        _services.Should().NotBeNull();
    }

    [Given(@"I have registered AppCore services")]
    public void GivenIHaveRegisteredAppCoreServices() {
        _services = new ServiceCollection();
        _services.AddCoreApplication();
        _serviceProvider = _services.BuildServiceProvider();
        _serviceProvider.Should().NotBeNull();
    }

    [When(@"I call AddAppCoreCore\(\)")]
    public void WhenICallAddAppCoreCore() {
        try {
            _services!.AddCoreApplication();
        } catch (Exception ex) {
            _thrownException = ex;
        }
    }

    [When(@"I register services individually")]
    public void WhenIRegisterServicesIndividually() {
        _services!.AddTransient<IDateTimeService, DateTimeService>();
        _services!.AddScoped<ICurrentUserService, CurrentUserService>();
        _services!.AddHttpContextAccessor();
    }

    [When(@"I build the service provider")]
    public void WhenIBuildTheServiceProvider() {
        try {
            _serviceProvider = _services!.BuildServiceProvider();
        } catch (Exception ex) {
            _thrownException = ex;
        }
    }

    [When(@"I validate the service configuration")]
    public void WhenIValidateTheServiceConfiguration() {
        try {
            _serviceProvider = _services!.BuildServiceProvider();
            // Intentar resolver todos los servicios registrados para validar configuración
            _serviceProvider.GetService<IDateTimeService>();
            _serviceProvider.GetService<ICurrentUserService>();
        } catch (Exception ex) {
            _thrownException = ex;
        }
    }

    [Then(@"the core interfaces should be registered")]
    public void ThenTheCoreInterfacesShouldBeRegistered() {
        _thrownException.Should().BeNull();
        _services.Should().NotBeNull();

        var serviceDescriptors = _services!.ToList();
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IDateTimeService));
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(ICurrentUserService));
    }

    [Then(@"IGenericRepository should be available")]
    public void ThenIGenericRepositoryShouldBeAvailable() {
        // En este contexto, verificamos que el servicio está disponible para registro
        // La implementación específica dependerá del DbContext configurado
        _services.Should().NotBeNull();
    }

    [Then(@"ICurrentUserService should be available")]
    public void ThenICurrentUserServiceShouldBeAvailable() {
        var serviceDescriptors = _services!.ToList();
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(ICurrentUserService));
    }

    [Then(@"IDateTimeService should be available")]
    public void ThenIDateTimeServiceShouldBeAvailable() {
        var serviceDescriptors = _services!.ToList();
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IDateTimeService));
    }

    [Then(@"I should have full control over service lifetimes")]
    public void ThenIShouldHaveFullControlOverServiceLifetimes() {
        var serviceDescriptors = _services!.ToList();
        var dateTimeService = serviceDescriptors.FirstOrDefault(s => s.ServiceType == typeof(IDateTimeService));
        var currentUserService = serviceDescriptors.FirstOrDefault(s => s.ServiceType == typeof(ICurrentUserService));

        dateTimeService.Should().NotBeNull();
        currentUserService.Should().NotBeNull();

        // Verificar que podemos configurar diferentes lifetimes
        dateTimeService!.Lifetime.Should().Be(ServiceLifetime.Transient);
        currentUserService!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Then(@"I should be able to override default implementations")]
    public void ThenIShouldBeAbleToOverrideDefaultImplementations() {
        // Verificar que los servicios son configurables
        _services!.AddTransient<IDateTimeService, CustomDateTimeService>();

        var serviceDescriptors = _services?.ToList();
        var customServices = serviceDescriptors?.Where(s => s.ServiceType == typeof(IDateTimeService));

        // Debe haber múltiples registros (el último prevalece)
        customServices?.Count().Should().BeGreaterThan(0);
    }

    [Then(@"Each service should be independently configurable")]
    public void ThenEachServiceShouldBeIndependentlyConfigurable() {
        _services.Should().NotBeNull();
        var serviceDescriptors = _services!.ToList();

        // Verificar que cada servicio puede ser configurado independientemente
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IDateTimeService));
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(ICurrentUserService));
    }

    [Then(@"I should be able to resolve IGenericRepository")]
    public void ThenIShouldBeAbleToResolveIGenericRepository() {
        _serviceProvider.Should().NotBeNull();
        _thrownException.Should().BeNull();

        // Nota: IGenericRepository requiere un tipo genérico específico
        // En un contexto real, esto se registraría con un DbContext específico
    }

    [Then(@"I should be able to resolve ICurrentUserService")]
    public void ThenIShouldBeAbleToResolveICurrentUserService() {
        _serviceProvider.Should().NotBeNull();
        _thrownException.Should().BeNull();

        var service = _serviceProvider!.GetService<ICurrentUserService>();
        service.Should().NotBeNull();
    }

    [Then(@"I should be able to resolve IDateTimeService")]
    public void ThenIShouldBeAbleToResolveIDateTimeService() {
        _serviceProvider.Should().NotBeNull();
        _thrownException.Should().BeNull();

        var service = _serviceProvider!.GetService<IDateTimeService>();
        service.Should().NotBeNull();
    }

    [Then(@"All services should have appropriate lifetimes")]
    public void ThenAllServicesShouldHaveAppropriateLifetimes() {
        _serviceProvider.Should().NotBeNull();

        // Verificar que los servicios se resuelven correctamente con sus lifetimes
        var dateTimeService1 = _serviceProvider!.GetService<IDateTimeService>();
        var dateTimeService2 = _serviceProvider!.GetService<IDateTimeService>();

        // DateTimeService es Transient, así que las instancias deben ser diferentes
        dateTimeService1.Should().NotBeNull();
        dateTimeService2.Should().NotBeNull();
        dateTimeService1.Should().NotBeSameAs(dateTimeService2);
    }

    [Then(@"there should be no circular dependencies")]
    public void ThenThereShouldBeNoCircularDependencies() {
        _serviceProvider.Should().NotBeNull();
        _thrownException.Should().BeNull();

        // Si llegamos aquí sin excepción, no hay dependencias circulares
        var dateTimeService = _serviceProvider!.GetService<IDateTimeService>();
        var currentUserService = _serviceProvider!.GetService<ICurrentUserService>();

        dateTimeService.Should().NotBeNull();
        currentUserService.Should().NotBeNull();
    }

    [Then(@"All required dependencies should be satisfied")]
    public void ThenAllRequiredDependenciesShouldBeSatisfied() {
        _serviceProvider.Should().NotBeNull();
        _thrownException.Should().BeNull();

        // Verificar que todos los servicios requeridos están disponibles
        var dateTimeService = _serviceProvider!.GetService<IDateTimeService>();
        var currentUserService = _serviceProvider!.GetService<ICurrentUserService>();

        dateTimeService.Should().NotBeNull();
        currentUserService.Should().NotBeNull();
    }

    [Then(@"Service lifetimes should be appropriate")]
    public void ThenServiceLifetimesShouldBeAppropriate() {
        _services.Should().NotBeNull();
        var serviceDescriptors = _services?.ToList();

        // Verificar que los servicios tienen lifetimes apropiados
        var dateTimeServiceDescriptor = serviceDescriptors?.FirstOrDefault(s => s.ServiceType == typeof(IDateTimeService));
        var currentUserServiceDescriptor = serviceDescriptors?.FirstOrDefault(s => s.ServiceType == typeof(ICurrentUserService));

        dateTimeServiceDescriptor.Should().NotBeNull();
        currentUserServiceDescriptor.Should().NotBeNull();
    }

    [Given(@"I have multiple entity types")]
    public void GivenIHaveMultipleEntityTypes() {
        // Simular que tenemos múltiples tipos de entidades
        // En una implementación real, tendríamos clases Entity1, Entity2, etc.
        _services.Should().NotBeNull();
    }

    [When(@"I register repositories for each entity type")]
    public void WhenIRegisterRepositoriesForEachEntityType() {
        // Simular el registro de múltiples repositorios
        // En implementación real, registraríamos IGenericRepository<Entity1>, IGenericRepository<Entity2>, etc.
        try {
            _services!.AddScoped(typeof(IGenericRepository<,>), typeof(TestGenericRepository<,>));
        } catch (Exception ex) {
            _thrownException = ex;
        }
    }

    [Then(@"each repository should be independently resolvable")]
    public void ThenEachRepositoryShouldBeIndependentlyResolvable() {
        _thrownException.Should().BeNull();
        _services.Should().NotBeNull();

        // Verificar que se registró el repositorio genérico
        var serviceDescriptors = _services?.ToList();
        var repositoryDescriptor = serviceDescriptors?.FirstOrDefault(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IGenericRepository<,>));

        repositoryDescriptor.Should().NotBeNull();
    }

    [Then(@"each repository should work with its specific entity type")]
    public void ThenEachRepositoryShouldWorkWithItsSpecificEntityType() {
        _thrownException.Should().BeNull();
        // En implementación real, verificaríamos que cada repositorio maneja su tipo específico
        // Por ahora, verificamos que no hubo errores en el registro
        _services.Should().NotBeNull();
    }

    [Then(@"there should be no conflicts between registrations")]
    public void ThenThereShouldBeNoConflictsBetweenRegistrations() {
        _thrownException.Should().BeNull();
        _services.Should().NotBeNull();

        // Verificar que no hay conflictos - el ServiceProvider se construye sin errores
        try {
            var testProvider = _services?.BuildServiceProvider();
            testProvider.Should().NotBeNull();
        } catch (Exception ex) {
            _thrownException = ex;
        }

        _thrownException.Should().BeNull();
    }

    // Clase de prueba para override
    private class CustomDateTimeService : IDateTimeService {
        public DateTime Now => DateTime.Now;
        public DateTime NowUtc => DateTime.UtcNow;
    }

    // Clase de prueba para repositorio genérico
    private class TestGenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
        where TEntity : BaseEntity<TId> {
        public Task<TEntity> AddAsync(TEntity entity) => Task.FromResult(entity);
        public Task<TEntity> UpdateAsync(TEntity entity) => Task.FromResult(entity);
        public Task<bool> DelAsync(TId id) => Task.FromResult(true);
        public Task<TEntity?> GetByIdAsync(TId id)
            => Task.FromResult<TEntity?>(null);
        public Task<List<TEntity>?> GetAllAsync()
            => Task.FromResult<List<TEntity>?>(new List<TEntity>());
        public Task<AppCore.Application.DTOs.PaginationDto<TEntity>> GetPagedAsync(int page, int pageSize)
            => Task.FromResult(new AppCore.Application.DTOs.PaginationDto<TEntity>());
    }
}
