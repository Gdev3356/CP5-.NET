using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Contra_Filé.API.HealthChecks;

/// <summary>
/// Verifica a disponibilidade de uma URL externa (site do Contra Filé).
/// Usa um HttpClient nomeado registrado em AddContraFiléHealthChecks, com timeout curto para não
/// travar o /health caso o site externo esteja lento.
/// </summary>
public sealed class ContraFileSiteHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ContraFileSiteHealthCheck");
            using var response = await client.GetAsync("https://www.fiap.com.br", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Site da FIAP respondendo normalmente.")
                : HealthCheckResult.Unhealthy($"Site da FIAP retornou status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            // Propositalmente Unhealthy (não Degraded): serve para o grupo observar, como o
            // enunciado pede, que uma dependência externa fora do ar derruba o /health inteiro.
            return HealthCheckResult.Unhealthy("Não foi possível acessar o site da FIAP.", ex);
        }
    }
}