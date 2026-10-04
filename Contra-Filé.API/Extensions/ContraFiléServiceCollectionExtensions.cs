using Contra_Filé.Application.Repositories;
using Contra_Filé.Application.Services;
using Contra_Filé.Infrastructure.Persistence;
using Contra_Filé.Infrastructure.Persistence.Repositories;

namespace Contra_Filé.Extensions;

using System.Reflection;
using System.Text.Json;
using Asp.Versioning;
using Contra_Filé.API.Exceptions;
using Contra_Filé.API.HealthChecks;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Application.Services;
using Contra_Filé.Infrastructure.Persistence;
using Contra_Filé.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

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

        services.AddDbContext<ContraFiléContext>(opt =>
            opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

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

    /// <summary>
    /// Swagger/OpenAPI com XML comments.
    /// Trocou o SwaggerDoc("v1", ...) único por um documento por versão de API, via
    /// <see cref="ConfigureSwaggerOptions"/> (precisa que AddContraFiléApiVersioning já tenha
    /// sido chamado, pois depende do IApiVersionDescriptionProvider).
    /// </summary>
    public static IServiceCollection AddContraFiléSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });

        return services;
    }

    /// <summary>
    /// CP5: versionamento de API (Asp.Versioning). Versão default 2.0 quando a requisição não
    /// informa nenhuma; leitura por query string ("api-version") OU header ("X-Api-Version");
    /// ReportApiVersions=true inclui os headers "api-supported-versions" e
    /// "api-deprecated-versions" em toda resposta.
    /// </summary>
    public static IServiceCollection AddContraFiléApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(2, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Api-Version"));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVVV"; // v1.0, v2.0
            });

        return services;
    }

    /// <summary>
    /// CP5: rate limit fixed window (10 requisições/minuto) aplicado via
    /// [EnableRateLimiting("fixed")] só nas actions que o grupo decidir, NÃO é global, então
    /// GET /health nunca é afetado sem precisar de DisableRateLimiting em lugar nenhum.
    /// </summary>
    public static IServiceCollection AddContraFiléRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 10;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiter.QueueLimit = 0; // sem fila: estoura 429 na hora, não segura a requisição
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problem = new
                {
                    type = "about:blank",
                    title = "Muitas requisições",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Limite de 10 requisições por minuto excedido para este endpoint. Tente novamente em instantes.",
                    instance = context.HttpContext.Request.Path.Value
                };

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(problem), cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Tegistra os health checks da API, "self" (processo no ar), banco (MySQL via
    /// AddDbContextCheck sobre o mesmo DbContext do CP2) e um check externo recomendado.
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
    /// CP4: mapeia GET /health com um response writer JSON (status geral, duração total e cada
    /// check nomeado com seu próprio status/duração; detalhe de exceção só em Development) e os
    /// status HTTP corretos (Healthy/Degraded para 200, Unhealthy para 503).
    /// CP5: este endpoint NUNCA leva [EnableRateLimiting], fica de fora do teto de propósito.
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