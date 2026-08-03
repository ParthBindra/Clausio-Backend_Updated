using Clausio.Legal.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Infrastructure;

public class ClausioDbContext(DbContextOptions<ClausioDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Case> Cases => Set<Case>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ActionPlan> ActionPlans => Set<ActionPlan>();
    public DbSet<Contradiction> Contradictions => Set<Contradiction>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Hearing> Hearings => Set<Hearing>();
    public DbSet<HearingOrder> HearingOrders => Set<HearingOrder>();
    public DbSet<LegalResearch> LegalResearches => Set<LegalResearch>();
    public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();
    public DbSet<Readiness> Readinesses => Set<Readiness>();
    public DbSet<ReadinessChecklistItem> ReadinessChecklistItems => Set<ReadinessChecklistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClausioDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void TouchTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
