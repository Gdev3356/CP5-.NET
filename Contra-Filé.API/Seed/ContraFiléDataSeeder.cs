using Contra_Filé.Domain.Entities;
using Contra_Filé.Infrastructure.Persistence;

namespace Contra_Filé.API.Seed;

/// <summary>
/// Popula o banco com dados de exemplo, só em ambiente de Development e só se o banco
/// estiver vazio (idempotente — roda de novo em todo restart, mas não duplica nada).
/// Usa os construtores de verdade das entidades: todas as regras de negócio são respeitadas,
/// inclusive o hash de senha do User (via HashHelper, chamado dentro do próprio construtor).
/// </summary>
public static class ContraFiléDataSeeder
{
    public static void Seed(ContraFiléContext context)
    {
        try
        {
            if (context.Mesas.Any() || context.Users.Any() || context.Pratos.Any())
                return;

            // --- Entidades independentes (sem FK entre si) ---
            var mesas = new List<Mesa>
            {
                new(1, false),
                new(2, true),
                new(3, false),
                new(4, false),
            };
            context.Mesas.AddRange(mesas);

            var pratos = new List<Prato>
            {
                new("Picanha na Chapa",   79.90m, 25, "Picanha grelhada, acompanha farofa e vinagrete."),
                new("Feijoada Completa",  54.90m, 40, "Feijoada tradicional com acompanhamentos completos."),
                new("Filé Molho Madeira", 68.50m, 30, "Filé mignon ao molho madeira com purê de batata."),
                new("Salada Caesar",      32.00m, 10, "Alface, croutons, parmesão e molho caesar."),
            };
            context.Pratos.AddRange(pratos);

            var usuarios = new List<User>
            {
                new("Ana Souza",  "ana.souza@example.com",  new DateOnly(1990, 4, 12), "SenhaForte123"),
                new("Bruno Lima", "bruno.lima@example.com", new DateOnly(1985, 9, 3),  "SenhaForte123"),
                new("Carla Dias", "carla.dias@example.com", new DateOnly(1998, 1, 27), "SenhaForte123"),
            };
            context.Users.AddRange(usuarios);

            context.SaveChanges();

            // --- Entidades que dependem dos Ids acima ---
            var configs = usuarios.Select(u => new UserConfig(u.Id)).ToList();
            context.UserConfigs.AddRange(configs);

            var pedidos = new List<Pedido>
            {
                new(154.80m, mesas[1].Id, usuarios[0].Id),
                new(68.50m,  mesas[1].Id, usuarios[1].Id),
            };
            context.Pedidos.AddRange(pedidos);

            var avaliacoes = new List<Avaliacao>
            {
                new(Guid.NewGuid(), usuarios[0].Id, 5, "Atendimento excelente, comida impecável."),
                new(Guid.NewGuid(), usuarios[1].Id, 4, "Muito bom, só demorou um pouco pra servir."),
            };
            context.Avaliacaos.AddRange(avaliacoes);

            context.SaveChanges();
        }
        catch (Exception)
        {
            // Silencia a exceção se o banco estiver fora do ar na inicialização,
            // garantindo que a API suba para o Health Check conseguir testar e retornar Unhealthy.
        }
    }
}