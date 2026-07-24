namespace Clausio.Legal.Core.Entities;

public class Hearing : BaseEntity
{
    public DateTime HearingDate { get; set; }
    public string? Stage { get; set; }
    public string? Judge { get; set; }
    public string? CourtHall { get; set; }
    public string? WhatHappened { get; set; }
    public string? JudgeObservation { get; set; }
    public string? OpposingAdmission { get; set; }
    public string? NextObjective { get; set; }

    public Guid CaseId { get; set; }
    public Case? Case { get; set; }

    public ICollection<HearingOrder> Orders { get; set; } = new List<HearingOrder>();
}
