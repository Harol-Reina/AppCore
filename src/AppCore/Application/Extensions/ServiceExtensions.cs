using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace AppCore.Application.Extensions;

public static class ServiceExtensions {

    public static void AddOpenApiExtension(this IServiceCollection services, IConfiguration configuration) {
        // Read configuration eagerly for fail-fast validation
        var version = configuration["OpenApiInfo:Version"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Version' is not set.");
        var title = configuration["OpenApiInfo:Title"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Title' is not set.");
        var description = configuration["OpenApiInfo:Description"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Description' is not set.");
        var contactName = configuration["OpenApiInfo:Contact:Name"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Contact:Name' is not set.");
        var contactEmail = configuration["OpenApiInfo:Contact:Email"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Contact:Email' is not set.");
        var contactUrl = new Uri(configuration["OpenApiInfo:Contact:Url"] ?? throw new KeyNotFoundException("Configuration key 'OpenApiInfo:Contact:Url' is not set."));

        services.AddOpenApi("v1", options => {
            options.AddDocumentTransformer((document, context, cancellationToken) => {
                document.Info = new OpenApiInfo {
                    Version = version,
                    Title = title,
                    Description = description,
                    Contact = new OpenApiContact {
                        Name = contactName,
                        Email = contactEmail,
                        Url = contactUrl
                    }
                };
                return Task.CompletedTask;
            });
        });
    }

    [Obsolete("Use AddOpenApiExtension instead. Swashbuckle has been replaced with Microsoft.AspNetCore.OpenApi for AOT compatibility.")]
    public static void AddSwaggerExtension(this IServiceCollection services, IConfiguration configuration) =>
        services.AddOpenApiExtension(configuration);

    public static void AddCorsExtension(this IServiceCollection services, IConfiguration configuration) {
        var section = configuration.GetSection("Cors:Origins");
        if (!section.Exists())
            throw new KeyNotFoundException("Configuration key 'Cors:Origins' is not set.");
        var origins = section.GetChildren().Select(x => x.Value).Where(x => x != null).Cast<string>().ToArray();

        services.AddCors(options => {
            options.AddPolicy("prod",
            builder => {
                builder.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            });
        });

        services.AddCors(options => {
            options.AddPolicy("dev",
            builder => {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });
    }
}
