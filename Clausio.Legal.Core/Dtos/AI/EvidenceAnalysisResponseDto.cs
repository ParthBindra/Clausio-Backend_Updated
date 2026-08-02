namespace Clausio.Legal.Core.Dtos.AI;

public class EvidenceAnalysisResponseDto
{
    public string Summary { get; set; } = string.Empty;

    public string EvidentiaryValue { get; set; } = string.Empty;

    public List<string> Strengths { get; set; } = [];

    public List<string> Weaknesses { get; set; } = [];

    public List<string> Recommendations { get; set; } = [];
}