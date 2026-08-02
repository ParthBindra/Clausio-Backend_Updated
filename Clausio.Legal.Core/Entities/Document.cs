namespace Clausio.Legal.Core.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? ExhibitLabel { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? ExtractedText { get; set; }

    public Guid CaseId { get; set; }
    public Case? Case { get; set; }
}