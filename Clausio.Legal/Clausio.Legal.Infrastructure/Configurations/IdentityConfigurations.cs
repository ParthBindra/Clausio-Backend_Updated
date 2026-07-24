using Clausio.Legal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clausio.Legal.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(c => c.MonthlyIncome).HasColumnType("numeric(18,2)");
        builder.HasMany(c => c.Cases)
            .WithOne(c => c.Client)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
