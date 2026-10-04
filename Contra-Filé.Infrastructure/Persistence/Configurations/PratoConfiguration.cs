namespace Contra_Filé.Infrastructure.Persistence.Configurations;
using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PratoConfiguration : IEntityTypeConfiguration<Prato>
{
    public void Configure(EntityTypeBuilder<Prato> builder)
    {
        builder.ToTable("Pratos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(p => p.Description).IsRequired().HasMaxLength(100);
        builder.Property(p => p.TimePrepareMinutes).IsRequired();
    }
}