namespace Clausio.Legal.Core.Dtos;

public class CreateCaseDto
{
    public string? Name { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseType { get; set; }
    public string? SubType { get; set; }
    public string? Court { get; set; }
    public string? CourtLocation { get; set; }
    public string? Stage { get; set; }
    public string? Priority { get; set; }
    public string? OpposingAdv { get; set; }
    public DateTime FiledOn { get; set; }
    public DateTime? NextHearing { get; set; }
    public Guid ClientId { get; set; }
}

public class UpdateCaseDto
{
    public string? Name { get; set; }
    public string? Stage { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? OpposingAdv { get; set; }
    public DateTime? NextHearing { get; set; }
    public int? ReadinessScore { get; set; }
}
