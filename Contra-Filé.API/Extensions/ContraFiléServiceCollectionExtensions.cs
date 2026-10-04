using Contra_Filé.Application.Repositories;
using Contra_Filé.Application.Services;
using Contra_Filé.HealthChecks;
using Contra_Filé.Infrastructure.Persistence;
using Contra_Filé.Infrastructure.Persistence.Repositories;

namespace Contra_Filé.Extensions;

using System.Reflection;
using System.Text.Json;
using Contra_Filé.API.Exceptions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Application.Services;
using Contra_Filé.Infrastructure.Persistence;
using Contra_Filé.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

/// <summary>
/// Extensões de DI para manter o Program.cs enxuto.
/// </summary>
public static class ContraFiléServiceCollectionExtensions
{
    /// <summary>Registra o DbContext com MySQL.</summary>
    public static IServiceCollection AddContraFiléDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("ContraFiléMySQL")
                 ?? throw new InvalidOperationException(
                     "Connection string 'ContraFiléMySQL' não encontrada.");

        // Informa a versão fixa (ex: MySQL 8.0) para evitar o AutoDetect na inicialização
        var version = new MySqlServerVersion(new Version(8, 0, 30));

        services.AddDbContext<ContraFiléContext>(opt =>
            opt.UseMySql(cs, version));

        return services;
    }

    /// <summary>Repositório genérico + serviços de aplicação.</summary>
    public static IServiceCollection AddContraFiléServices(this IServiceCollection services)
    {
        // Repositório genérico — cobre todas as entidades
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Serviços de aplicação
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IMesaService,   MesaService>();
        services.AddScoped<IPratoService,  PratoService>();

        return services;
    }

    /// <summary>Swagger/OpenAPI com XML comments.</summary>
    public static IServiceCollection AddContraFiléSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Contra-Filé API",
                Version     = "v1",
                Description = "API REST para gerenciamento de restaurante: mesas, pedidos e cardápio.",
                Contact     = new OpenApiContact
                {
                    Name = "Equipe Contra-Filé",
                    Email = "contato@contrafile.example.com"
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });

        return services;
    }

    /// <summary>
    /// Registra os health checks da API — "self" (processo no ar), banco (MySQL via
    /// AddDbContextCheck sobre o mesmo DbContext do CP2) e um check externo recomendado
    /// (disponibilidade do site da FIAP).
    /// </summary>
    public static IServiceCollection AddContraFiléHealthChecks(this IServiceCollection services)
    {
        services.AddHttpClient("ContraFileSiteHealthCheck", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API no ar."))
            .AddDbContextCheck<ContraFiléContext>(name: "mysql-database")
            .AddCheck<ContraFileSiteHealthCheck>("fiap-site");

        return services;
    }

    /// <summary>
    /// Mapeia GET /health com um response writer JSON (status geral, duração total e cada
    /// check nomeado com seu próprio status/duração; detalhe de exceção só em Development) e os
    /// status HTTP corretos (Healthy/Degraded → 200, Unhealthy → 503).
    /// </summary>
    public static WebApplication MapContraFiléHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy]   = StatusCodes.Status200OK,
                [HealthStatus.Degraded]  = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = WriteHealthReportAsync
        });

        return app;
    }

    private static Task WriteHealthReportAsync(HttpContext context, HealthReport report)
    {
        var isDevelopment = context.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = isDevelopment ? e.Value.Exception?.Message : null
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}