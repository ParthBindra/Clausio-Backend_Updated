namespace Clausio.Legal.Core.Dtos.AI;

public class CaseSummaryResponseDto
{
    public string CoreFacts { get; set; } = string.Empty;

    public string CurrentStage { get; set; } = string.Empty;

    public List<string> KeyStrengths { get; set; } = [];

    public List<string> KeyWeaknesses { get; set; } = [];

    public List<string> NextSteps { get; set; } = [];
}