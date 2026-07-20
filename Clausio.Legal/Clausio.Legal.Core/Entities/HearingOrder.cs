namespace Clausio.Legal.Core.Entities;

public class HearingOrder : BaseEntity
{
    public string? Text { get; set; }
    public string? Responsible { get; set; }
    public DateTime Deadline { get; set; }
    public bool Done { get; set; }

    public Guid HearingId { get; set; }
    public Hearing? Hearing { get; set; }
}
