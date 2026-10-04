using Contra_Filé.API.Exceptions;
using Contra_Filé.API.Seed;
using Contra_Filé.Extensions;
using Contra_Filé.Infrastructure.Persistence;

namespace Contra_Filé.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddContraFiléDbContext(builder.Configuration);
        builder.Services.AddContraFiléServices();
        builder.Services.AddContraFiléApiVersioning();  // CP5 — precisa vir antes do Swagger
        builder.Services.AddContraFiléSwagger();
        builder.Services.AddContraFiléHealthChecks();   // CP4
        builder.Services.AddContraFiléRateLimiting();   // CP5

        builder.Services.AddControllers();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // ExceptionHandler ANTES do roteamento
        app.UseExceptionHandler();

        // Depois do ExceptionHandler, antes do MapControllers (regra do enunciado).
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // Um endpoint de Swagger por versão descoberta (v1.0 deprecada, v2.0 atual).
                foreach (var description in app.DescribeApiVersions())
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"Contra-Filé API {description.GroupName}");
                }
                options.RoutePrefix = ""; // Swagger na raiz: http://localhost:5xxx/
            });

            // Popula o banco com dados de exemplo (só em Development, só se estiver vazio).
            using var seedScope = app.Services.CreateScope();
            var context = seedScope.ServiceProvider.GetRequiredService<ContraFiléContext>();
            ContraFiléDataSeeder.Seed(context);
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapContraFiléHealthChecks(); // GET /health — fora do Swagger e fora do rate limit

        app.Run();
    }
}