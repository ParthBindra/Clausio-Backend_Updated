namespace Clausio.Legal.Core.Entities;

public class ActionPlan : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public DateTime DueBy { get; set; }
    public string? AssignedTo { get; set; }
    public bool Done { get; set; }

    public Guid CaseId { get; set; }
    public Case? Case { get; set; }
}
