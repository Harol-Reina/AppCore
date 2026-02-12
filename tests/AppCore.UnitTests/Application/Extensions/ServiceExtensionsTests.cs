using AppCore.Application.Extensions;
using AppCore.Application.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions;

[Collection("ConfigurationTests")]
public class ServiceExtensionsTests {

    public ServiceExtensionsTests() {
        // Configuration initialization handled in specific tests
    }

    private static void SetupConfiguration(Dictionary<string, string?> settings) {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        Configuration.Initialize(configuration);
    }

    [Fact]
    public void AddOpenApiExtension_ShouldRegisterOpenApiServices() {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"OpenApiInfo:Version", "v1"},
            {"OpenApiInfo:Title", "Test API"},
            {"OpenApiInfo:Description", "Test Description"},
            {"OpenApiInfo:Contact:Name", "Test Contact"},
            {"OpenApiInfo:Contact:Email", "test@example.com"},
            {"OpenApiInfo:Contact:Url", "https://example.com"},
        };
        SetupConfiguration(settings);

        var services = new ServiceCollection();

        // Add Logging needed by OpenApi
        services.AddLogging();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w => w.ApplicationName == "TestApp"));
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(sp => sp.GetRequiredService<IWebHostEnvironment>());

        // Act
        services.AddOpenApiExtension();
        var provider = services.BuildServiceProvider();

        // Assert - AddOpenApi registers internal OpenApi services
        services.Should().Contain(sd => sd.ServiceType.FullName!.Contains("OpenApi"));
    }

    [Fact]
    public void AddCorsExtension_ShouldRegisterCorsServices() {
        // Arrange
        var settings = new Dictionary<string, string?> {
            {"Cors:Origins:0", "https://localhost:3000"},
            {"Cors:Origins:1", "https://example.com"}
        };
        SetupConfiguration(settings);

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCorsExtension();
        var provider = services.BuildServiceProvider();

        // Assert
        var corsService = provider.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();
        corsService.Should().NotBeNull();

        var policyProvider = provider.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>();
        policyProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenApiExtension_WithMissingConfig_ShouldThrowException() {
        // Arrange
        var settings = new Dictionary<string, string?>(); // Empty
        SetupConfiguration(settings);

        var services = new ServiceCollection();

        // Act - AddOpenApiExtension triggers configuration read immediately via document transformer registration
        var act = () => services.AddOpenApiExtension();

        // Assert
        act.Should().Throw<AppCore.Application.Exceptions.NotFoundException>()
           .WithMessage("*OpenApiInfo:Version*");
    }

    [Fact]
    public void AddCorsExtension_WithMissingConfig_ShouldThrowException() {
        // Arrange
        var settings = new Dictionary<string, string?>(); // Empty
        SetupConfiguration(settings);

        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddLogging();

        // Act
        services.AddCorsExtension();
        var provider = services.BuildServiceProvider();

        // Assert
        // Resolution itself might trigger exception if Options are accessed in constructor
        Func<Task> act = async () => {
            var policyProvider = provider.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>();
            await policyProvider.GetPolicyAsync(new Microsoft.AspNetCore.Http.DefaultHttpContext(), "prod");
        };

        act.Should().ThrowAsync<AppCore.Application.Exceptions.NotFoundException>()
           .WithMessage("*Cors:Origins*");
    }
}
