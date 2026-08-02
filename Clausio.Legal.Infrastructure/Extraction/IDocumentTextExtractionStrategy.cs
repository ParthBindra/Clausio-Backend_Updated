namespace Clausio.Legal.Infrastructure.Extraction;

public interface IDocumentTextExtractionStrategy
{
    bool CanHandle(string fileExtension);
    Task<string?> ExtractAsync(string storagePath, CancellationToken cancellationToken = default);
}