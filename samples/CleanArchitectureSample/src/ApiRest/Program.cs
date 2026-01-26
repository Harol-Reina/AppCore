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
using App.ApiRest.Extensions;
using App.ApiRest;

var builder = WebApplication.CreateBuilder(args);
Configuration.Initialize(builder.Configuration);

builder.Host.UseSerilog(
    (context, configuration) 
    => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExtension();

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition
        = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SampleJsonContext.Default);
});
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

app.UseMiddleware<HttpClientCustomHandler>();
app.MapHealthChecks("/health", new HealthCheckOptions {
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
AppConstants.Init();

app.MapEndpoints();
app.Run();
