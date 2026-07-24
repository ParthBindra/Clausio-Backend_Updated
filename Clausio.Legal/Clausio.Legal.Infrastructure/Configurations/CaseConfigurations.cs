using Clausio.Legal.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clausio.Legal.Infrastructure.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.HasIndex(c => c.CaseNumber);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.ActionPlans).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Contradictions).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Documents).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Hearings).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.LegalResearches).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.TimelineEvents).WithOne(a => a.Case).HasForeignKey(a => a.CaseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Readiness).WithOne(r => r.Case).HasForeignKey<Readiness>(r => r.CaseId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActionPlanConfiguration : IEntityTypeConfiguration<ActionPlan>
{
    public void Configure(EntityTypeBuilder<ActionPlan> builder) { }
}

public class ContradictionConfiguration : IEntityTypeConfiguration<Contradiction>
{
    public void Configure(EntityTypeBuilder<Contradiction> builder) { }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(d => d.FileName).HasMaxLength(500).IsRequired();
        builder.Property(d => d.StoragePath).HasMaxLength(1000).IsRequired();
    }
}
