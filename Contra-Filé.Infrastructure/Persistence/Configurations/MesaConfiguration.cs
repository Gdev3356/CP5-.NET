namespace Contra_Filé.Infrastructure.Persistence.Configurations;
using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("Mesas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Number).IsRequired();
        builder.Property(m => m.IsOccupied).IsRequired();

        builder.HasMany(m => m.Pedidos)
            .WithOne()
            .HasForeignKey(p => p.MesaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}