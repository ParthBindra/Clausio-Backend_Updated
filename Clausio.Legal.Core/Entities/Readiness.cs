namespace Clausio.Legal.Core.Entities;

public class Readiness : BaseEntity
{
    public int     Score         { get; set; }
    public string? Summary       { get; set; }
    public string? GapsJson      { get; set; }
    public string? StrengthsJson { get; set; }

    public Guid  CaseId { get; set; }
    public Case? Case   { get; set; }

    public ICollection<ReadinessChecklistItem> ChecklistItems { get; set; } = new List<ReadinessChecklistItem>();
}
