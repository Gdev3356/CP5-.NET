namespace Contra_Filé.Infrastructure.Persistence.Configurations;
using Contra_Filé.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Password).IsRequired();

        builder.HasOne(u => u.Config)
            .WithOne()
            .HasForeignKey<UserConfig>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Avaliacoes)
            .WithOne()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}