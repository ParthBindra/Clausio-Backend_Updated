using Clausio.Legal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clausio.Legal.Infrastructure.Configurations;

public class LegalResearchConfiguration : IEntityTypeConfiguration<LegalResearch>
{
    public void Configure(EntityTypeBuilder<LegalResearch> builder) { }
}

public class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder) { }
}

public class ReadinessConfiguration : IEntityTypeConfiguration<Readiness>
{
    public void Configure(EntityTypeBuilder<Readiness> builder)
    {
        builder.HasMany(r => r.ChecklistItems)
            .WithOne(i => i.Readiness)
            .HasForeignKey(i => i.ReadinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReadinessChecklistItemConfiguration : IEntityTypeConfiguration<ReadinessChecklistItem>
{
    public void Configure(EntityTypeBuilder<ReadinessChecklistItem> builder) { }
}
