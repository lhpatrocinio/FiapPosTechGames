using Asp.Versioning.ApiExplorer;
using Games.Api.Extensions.Auth.Middleware;
using Games.Api.Extensions.Auth;
using Games.Api.Extensions.Logs.Extension;
using Games.Api.Extensions.Logs;
using Games.Api.Extensions.Migration;
using Games.Api.Extensions.Swagger.Middleware;
using Games.Api.Extensions.Swagger;
using Games.Application;
using Games.Infrastructure.DataBase.EntityFramework.Context;
using Games.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Games.Api.Extensions.Mappers;
using Games.Api.Extensions.Versioning;
using Games.Application.Consumers;
using Games.Infrastructure.Monitoring;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogConfiguration();
builder.WebHost.UseUrls("http://*:80");

builder.Services.AddMvcCore(options => options.AddLogRequestFilter());
builder.Services.AddVersioning();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorizationExtension(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


#region [DI]

ApplicationBootstrapper.Register(builder.Services);
InfraBootstrapper.Register(builder.Services, builder.Configuration);

// Prometheus monitoring
builder.Services.AddPrometheusMonitoring();

#endregion

#region [Consumers]

builder.Services.AddHostedService<UserCreatedConsumer>();

#endregion

var app = builder.Build();

app.ExecuteMigrations();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseAuthentication();                        // 1°: popula HttpContext.User
app.UseMiddleware<RoleAuthorizationMiddleware>();
app.UseCorrelationId();

app.UseCors("AllowAll");

// Prometheus middleware
app.UsePrometheusMonitoring();

app.UseVersionedSwagger(apiVersionDescriptionProvider);
app.UseAuthorization();                         // 3°: aplica [Authorize]
app.UseHttpsRedirection();
app.MapControllers();

// Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.Run();