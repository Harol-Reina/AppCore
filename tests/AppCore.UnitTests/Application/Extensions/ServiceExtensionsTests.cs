using OrionSoft.AppCore.Application.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Extensions;

[Collection("ConfigurationTests")]
public class ServiceExtensionsTests {

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> settings) {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void AddOpenApiExtension_ShouldRegisterOpenApiServices() {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?> {
            {"OpenApiInfo:Version", "v1"},
            {"OpenApiInfo:Title", "Test API"},
            {"OpenApiInfo:Description", "Test Description"},
            {"OpenApiInfo:Contact:Name", "Test Contact"},
            {"OpenApiInfo:Contact:Email", "test@example.com"},
            {"OpenApiInfo:Contact:Url", "https://example.com"},
        });

        var services = new ServiceCollection();

        // Add Logging needed by OpenApi
        services.AddLogging();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w => w.ApplicationName == "TestApp"));
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(sp => sp.GetRequiredService<IWebHostEnvironment>());

        // Act
        services.AddOpenApiExtension(configuration);
        var provider = services.BuildServiceProvider();

        // Assert - AddOpenApi registers internal OpenApi services
        services.Should().Contain(sd => sd.ServiceType.FullName!.Contains("OpenApi"));
    }

    [Fact]
    public void AddCorsExtension_ShouldRegisterCorsServices() {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?> {
            {"Cors:Origins:0", "https://localhost:3000"},
            {"Cors:Origins:1", "https://example.com"}
        });

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCorsExtension(configuration);
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
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        var services = new ServiceCollection();

        // Act
        var act = () => services.AddOpenApiExtension(configuration);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
           .WithMessage("*OpenApiInfo:Version*");
    }

    [Fact]
    public void AddCorsExtension_WithMissingConfig_ShouldThrowException() {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCorsExtension(configuration);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
           .WithMessage("*Cors:Origins*");
    }
}
