namespace Clausio.Legal.Core.Dtos;

public class CreateHearingDto
{
    public DateTime HearingDate { get; set; }
    public string? Stage { get; set; }
    public string? Judge { get; set; }
    public string? CourtHall { get; set; }
    public string? WhatHappened { get; set; }
    public string? JudgeObservation { get; set; }
    public string? OpposingAdmission { get; set; }
    public string? NextObjective { get; set; }
    public List<CreateHearingOrderDto>? Orders { get; set; }
}

public class CreateHearingOrderDto
{
    public string? Text { get; set; }
    public string? Responsible { get; set; }
    public DateTime Deadline { get; set; }
}

public class UpdateHearingDto
{
    public DateTime? HearingDate { get; set; }
    public string? Stage { get; set; }
    public string? Judge { get; set; }
    public string? CourtHall { get; set; }
    public string? WhatHappened { get; set; }
    public string? JudgeObservation { get; set; }
    public string? OpposingAdmission { get; set; }
    public string? NextObjective { get; set; }
}
