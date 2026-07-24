namespace Clausio.Legal.Core.Entities;

public class Contradiction : BaseEntity
{
    public string? Claim { get; set; }
    public string? ClaimSource { get; set; }
    public string? Evidence { get; set; }
    public string? EvidenceSource { get; set; }
    public string? CourtArgument { get; set; }
    public string? Strength { get; set; }
    public bool Used { get; set; }

    public Guid CaseId { get; set; }
    public Case? Case { get; set; }
}
