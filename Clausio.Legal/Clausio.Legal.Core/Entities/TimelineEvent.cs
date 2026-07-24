namespace Clausio.Legal.Core.Entities;

public class TimelineEvent : BaseEntity
{
    public DateTime EventDate { get; set; }
    public string? Event { get; set; }
    public string? Source { get; set; }
    public string? LegalSignificance { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }

    public Guid CaseId { get; set; }
    public Case? Case { get; set; }
}
