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
        builder.Services.AddContraFiléSwagger();
        builder.Services.AddContraFiléHealthChecks();

        builder.Services.AddControllers();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // ExceptionHandler ANTES do roteamento
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contra-Filé API v1");
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
        app.MapContraFiléHealthChecks(); // GET /health (não aparece no Swagger, não é um controller)

        app.Run();
    }
}