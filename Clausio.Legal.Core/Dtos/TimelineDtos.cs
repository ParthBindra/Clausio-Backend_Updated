namespace Clausio.Legal.Core.Dtos;

public class CreateTimelineEventDto
{
    public DateTime EventDate { get; set; }
    public string? Event { get; set; }
    public string? Source { get; set; }
    public string? LegalSignificance { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }
}

public class ReorderDto
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
