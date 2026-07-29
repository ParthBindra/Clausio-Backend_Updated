namespace Clausio.Legal.Core.Dtos;

public class CaseTypeRequestDto
{
    public string? Description { get; set; }
}

public class ChatRequestDto
{
    public string? Message { get; set; }
    public Guid? CaseId { get; set; }
    public List<string>? History { get; set; }
}

public class DraftRequestDto
{
    public string? DraftType { get; set; }
    public string? Instructions { get; set; }
}

public class EmergencyRequestDto
{
    public string? Query { get; set; }
}

public class TranslateRequest
{
    public string? Text { get; set; }
}

public class WhatsAppRequestDto
{
    public string? Tone { get; set; }
    public string? Language { get; set; }
}

// ✅ Updated — each AI feature returns correct field name
public class AiResultDto
{
    public string Result { get; set; } = string.Empty;
}

public class AiSummaryDto
{
    public string Summary { get; set; } = string.Empty;
}

public class AiResearchDto
{
    public string Judgments { get; set; } = string.Empty;
}

public class AiActionPlanDto
{
    public string ActionPlan { get; set; } = string.Empty;
}

public class AiWhatsAppDto
{
    public string Message { get; set; } = string.Empty;
}

public class AiFinancialDto
{
    public string Analysis { get; set; } = string.Empty;
}

public class AiReadinessDto
{
    public string Readiness { get; set; } = string.Empty;
}

public class AiTranslateDto
{
    public string TranslatedText { get; set; } = string.Empty;
    public string DetectedLanguage { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
}
