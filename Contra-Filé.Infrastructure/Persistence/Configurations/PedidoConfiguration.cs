namespace Contra_Filé.Infrastructure.Persistence.Configurations;
using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TotalValue)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        // N:N with Prato — join table required
        builder.HasMany(p => p.Pratos)
            .WithMany()
            .UsingEntity(j => j.ToTable("PedidoPratos"));
    }
}