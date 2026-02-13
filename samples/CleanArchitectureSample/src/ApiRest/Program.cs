using System.Text.Json;
using System.Text.Json.Serialization;
using App.Application.Common;
using AppCore.Application.Utils;
using AppCore.Application.Extensions;
using AppCore.Application.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using App.Application;
using App.Infrastructure;
using App.Infrastructure.Data;
using App.ApiRest.Extensions;
using App.ApiRest;

var builder = WebApplication.CreateSlimBuilder(args);
Configuration.Initialize(builder.Configuration);

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
    options.TypeInfoResolverChain.Add(AppCore.Application.Serialization.AppCoreJsonContext.Default);
    
    return options;
});

// Configure AppCore to use the same options globally
var globalOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    PropertyNameCaseInsensitive = true
};
globalOptions.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
globalOptions.TypeInfoResolverChain.Add(AppCore.Application.Serialization.AppCoreJsonContext.Default);
JsonExtend.Options = globalOptions;

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExtension();

builder.Services.ConfigureHttpJsonOptions(options => {
    var jsonOptions = options.SerializerOptions;
    jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
    jsonOptions.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
    jsonOptions.TypeInfoResolverChain.Add(AppCore.Application.Serialization.AppCoreJsonContext.Default);
});

// CreateSlimBuilder does not register regex constraint by default. We need it for Swagger or specific routes.
builder.Services.Configure<RouteOptions>(options => options.SetParameterPolicy<Microsoft.AspNetCore.Routing.Constraints.RegexInlineRouteConstraint>("regex"));

builder.Services.AddCorsExtension();
builder.Services.AddHealthChecks();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
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
AppConstants.Init();

using (var scope = app.Services.CreateScope()) {
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitAsync();
}

app.MapEndpoints();
app.Run();
