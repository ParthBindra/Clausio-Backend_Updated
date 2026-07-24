namespace Clausio.Legal.Core.Entities;

public class Case : BaseEntity
{
    public string? Name { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseType { get; set; }
    public string? SubType { get; set; }
    public string? Court { get; set; }
    public string? CourtLocation { get; set; }
    public string? Stage { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? OpposingAdv { get; set; }
    public DateTime FiledOn { get; set; }
    public DateTime? NextHearing { get; set; }
    public int? ReadinessScore { get; set; }

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<ActionPlan> ActionPlans { get; set; } = new List<ActionPlan>();
    public ICollection<Contradiction> Contradictions { get; set; } = new List<Contradiction>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Hearing> Hearings { get; set; } = new List<Hearing>();
    public ICollection<LegalResearch> LegalResearches { get; set; } = new List<LegalResearch>();
    public ICollection<TimelineEvent> TimelineEvents { get; set; } = new List<TimelineEvent>();
    public Readiness? Readiness { get; set; }
}
