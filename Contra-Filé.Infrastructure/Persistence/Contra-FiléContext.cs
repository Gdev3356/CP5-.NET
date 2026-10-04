using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contra_Filé.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core do projeto do Contra-Filé.
/// Configura o mapeamento das entidades para o banco de dados usado. Não é cópia viu Prof! Só seguindo os exemplos!
/// </summary>
public class ContraFiléContext(DbContextOptions<ContraFiléContext> options) : DbContext(options)
{
    public DbSet<Avaliacao> Avaliacaos { get; set; }
    
    public DbSet<Mesa> Mesas { get; set; }
    
    public DbSet<Pedido> Pedidos { get; set; }
    
    public DbSet<Prato> Pratos { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserConfig>  UserConfigs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContraFiléContext).Assembly);
    } 
}