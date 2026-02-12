using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace AppCore.Application.Extensions;

public static class ServiceExtensions {

    public static void AddOpenApiExtension(this IServiceCollection services) {
        // Read configuration eagerly for fail-fast validation
        var version = Utils.Configuration.RequiredConfig("OpenApiInfo:Version");
        var title = Utils.Configuration.RequiredConfig("OpenApiInfo:Title");
        var description = Utils.Configuration.RequiredConfig("OpenApiInfo:Description");
        var contactName = Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Name");
        var contactEmail = Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Email");
        var contactUrl = new Uri(Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Url"));

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
    public static void AddSwaggerExtension(this IServiceCollection services) =>
        services.AddOpenApiExtension();

    public static void AddCorsExtension(this IServiceCollection services) {
        services.AddCors(options => {
            options.AddPolicy("prod",
            builder => {
                builder.WithOrigins(Utils.Configuration.StringArray("Cors:Origins"))
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
