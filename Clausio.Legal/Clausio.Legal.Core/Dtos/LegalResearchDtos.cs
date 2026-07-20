namespace Clausio.Legal.Core.Dtos;

public class CreateLegalResearchDto
{
    public string? Citation { get; set; }
    public string? Court { get; set; }
    public int Year { get; set; }
    public string? RatioDecidendi { get; set; }
    public string? Relevance { get; set; }
    public string? HowToUse { get; set; }
    public string? Strength { get; set; }
    public string? FullJudgmentUrl { get; set; }
}
