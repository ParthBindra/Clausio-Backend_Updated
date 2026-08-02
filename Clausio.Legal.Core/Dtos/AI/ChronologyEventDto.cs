namespace Clausio.Legal.Core.Dtos.AI;

public class ChronologyEventDto
{
    public DateTime? EventDate { get; set; }

    public string Event { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string LegalSignificance { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}