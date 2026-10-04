using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Contra_Filé.Extensions;

/// <summary>
/// Gera um SwaggerDoc por versão descoberta pelo <see cref="IApiVersionDescriptionProvider"/>
/// (um grupo por versão de API), no lugar do único "v1" fixo que existia até o CP4.
/// </summary>
public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title       = "Contra-Filé API",
                    Version     = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated
                        ? "Esta versão está DEPRECADA. Use a versão mais recente quando possível."
                        : "API REST para gerenciamento de restaurante: mesas, pedidos e cardápio.",
                    Contact = new OpenApiContact
                    {
                        Name  = "Equipe Contra-Filé",
                        Email = "contato@contrafile.example.com"
                    }
                });
        }
    }
}