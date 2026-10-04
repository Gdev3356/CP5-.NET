namespace Contra_Filé.Infrastructure.Persistence.Configurations;
using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("Avaliacoes");
        builder.HasKey(a => a.AvaliacaoId);
        builder.Property(a => a.Score).IsRequired();
        builder.Property(a => a.Description).IsRequired().HasMaxLength(100);
    }
}