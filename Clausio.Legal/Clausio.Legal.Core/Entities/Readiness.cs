namespace Clausio.Legal.Core.Entities;

public class Readiness : BaseEntity
{
    public int     Score         { get; set; }
    public string? Summary       { get; set; }  // ✅ NEW — AI summary text
    public string? GapsJson      { get; set; }  // ✅ NEW — JSON array of gaps
    public string? StrengthsJson { get; set; }  // ✅ NEW — JSON array of strengths

    public Guid   CaseId { get; set; }
    public Case?  Case   { get; set; }

    public ICollection<ReadinessChecklistItem> ChecklistItems { get; set; } = new List<ReadinessChecklistItem>();
}
