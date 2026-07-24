namespace Clausio.Legal.Core.Dtos;

public class CreateContradictionDto
{
    public string? Claim { get; set; }
    public string? ClaimSource { get; set; }
    public string? Evidence { get; set; }
    public string? EvidenceSource { get; set; }
    public string? CourtArgument { get; set; }
    public string? Strength { get; set; }
}
