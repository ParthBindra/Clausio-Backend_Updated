namespace Clausio.Legal.Core.Dtos;

public class CreateActionPlanDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public DateTime DueBy { get; set; }
    public string? AssignedTo { get; set; }
}
