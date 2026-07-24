using Clausio.Legal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clausio.Legal.Infrastructure.Configurations;

public class HearingConfiguration : IEntityTypeConfiguration<Hearing>
{
    public void Configure(EntityTypeBuilder<Hearing> builder)
    {
        builder.HasMany(h => h.Orders)
            .WithOne(o => o.Hearing)
            .HasForeignKey(o => o.HearingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HearingOrderConfiguration : IEntityTypeConfiguration<HearingOrder>
{
    public void Configure(EntityTypeBuilder<HearingOrder> builder) { }
}
