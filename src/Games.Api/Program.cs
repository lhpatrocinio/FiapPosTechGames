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

// Adiciona configuração CORS para permitir solicitações do Prometheus
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//// Adiciona monitoramento com Prometheus
//builder.Services.AddPrometheusMonitoring();
//builder.Services.AddMetricsCollector();


#region [DI]

ApplicationBootstrapper.Register(builder.Services);
InfraBootstrapper.Register(builder.Services);

#endregion

#region [Consumers]

builder.Services.AddHostedService<UserCreatedConsumer>();

#endregion

var app = builder.Build();

app.ExecuteMigrations();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseAuthentication();                        // 1�: popula HttpContext.User
app.UseMiddleware<RoleAuthorizationMiddleware>(); // 2�: seu middleware
app.UseCorrelationId();

// Adiciona CORS antes de outros middlewares
app.UseCors("AllowAll");

//// Adiciona middleware de monitoramento
//app.UsePrometheusMonitoring();
//app.UseMetricsMiddleware();

app.UseVersionedSwagger(apiVersionDescriptionProvider);
app.UseAuthorization();                         // 3�: aplica [Authorize]
app.UseHttpsRedirection();
app.MapControllers();
app.Run();