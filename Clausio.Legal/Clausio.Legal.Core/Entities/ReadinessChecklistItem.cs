namespace Clausio.Legal.Core.Entities;

public class ReadinessChecklistItem : BaseEntity
{
    public string? Text { get; set; }
    public string? Category { get; set; }
    public bool Done { get; set; }

    public Guid ReadinessId { get; set; }
    public Readiness? Readiness { get; set; }
}
