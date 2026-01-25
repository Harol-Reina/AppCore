using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace AppCore.Application.Extensions;

public static class ServiceExtensions {

    private const string BearerScheme = "Bearer";

    public static void AddSwaggerExtension(this IServiceCollection services) {
        services.AddSwaggerGen(c => {
            XmlCommentsFilePaths.ForEach(xmlfile => c.IncludeXmlComments(xmlfile));
            c.SwaggerDoc("v1", new OpenApiInfo {
                Version = Utils.Configuration.RequiredConfig("OpenApiInfo:Version"),
                Title = Utils.Configuration.RequiredConfig("OpenApiInfo:Title"),
                Description = Utils.Configuration.RequiredConfig("OpenApiInfo:Description"),
                Contact = new OpenApiContact {
                    Name = Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Name"),
                    Email = Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Email"),
                    Url = new Uri(Utils.Configuration.RequiredConfig("OpenApiInfo:Contact:Url"))
                }
            });

        });
    }

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

    static List<string> XmlCommentsFilePaths {
        get =>
            [.. Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)];
    }
}
