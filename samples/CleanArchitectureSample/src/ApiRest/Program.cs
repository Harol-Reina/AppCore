using System.Text.Json;
using System.Text.Json.Serialization;
using App.Application.Common;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using App.Application;
using App.Infrastructure;
using App.Infrastructure.Data;
using App.ApiRest.Extensions;
using App.ApiRest;

var builder = WebApplication.CreateSlimBuilder(args);

// Bind AppSettings from configuration
var appSettings = new AppSettings {
    DefaultConnection = builder.Configuration["DefaultConnection"] ?? throw new KeyNotFoundException("Configuration key 'DefaultConnection' is not set."),
    SchemaDB = builder.Configuration["SchemaDB"] ?? throw new KeyNotFoundException("Configuration key 'SchemaDB' is not set."),
    PokemonHost = builder.Configuration["PokemonHost"] ?? "https://pokeapi.co/api/v2/"
};
builder.Services.AddSingleton(appSettings);

builder.Host.UseSerilog(
    (context, configuration)
    => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

builder.Services.AddHttpContextAccessor();

// Configurar JsonSerializerOptions como singleton para que AppCore lo use
builder.Services.AddSingleton<JsonSerializerOptions>(provider => {
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };
    options.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
    options.TypeInfoResolverChain.Add(OrionSoft.AppCore.Application.Serialization.AppCoreJsonContext.Default);

    return options;
});

// Configure AppCore to use the same options globally
var globalOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    PropertyNameCaseInsensitive = true
};
globalOptions.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
globalOptions.TypeInfoResolverChain.Add(OrionSoft.AppCore.Application.Serialization.AppCoreJsonContext.Default);
JsonExtend.Options = globalOptions;

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddOpenApiExtension(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options => {
    var jsonOptions = options.SerializerOptions;
    jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
    jsonOptions.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
    jsonOptions.TypeInfoResolverChain.Add(OrionSoft.AppCore.Application.Serialization.AppCoreJsonContext.Default);
});

builder.Services.AddCorsExtension(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseCors("dev");
} else {
    app.UseCors("prod");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapHealthChecks("/health", new HealthCheckOptions {
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope()) {
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitAsync();
}

app.MapEndpoints();
app.Run();
