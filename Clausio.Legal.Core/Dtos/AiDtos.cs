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

public class AiResultDto
{
    public string Result { get; set; } = string.Empty;
}
