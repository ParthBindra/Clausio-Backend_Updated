namespace Clausio.Legal.Infrastructure.Extraction;

public class DocumentTextExtractor(IEnumerable<IDocumentTextExtractionStrategy> strategies) : IDocumentTextExtractor
{
    public async Task<string?> ExtractTextAsync(string storagePath, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var strategy = strategies.FirstOrDefault(s => s.CanHandle(extension));
        if (strategy is null)
        {
            // No strategy registered for this extension yet (e.g. .pdf, .docx).
            // Intentionally a no-op rather than a failure, keeping upload future-proof.
            return null;
        }

        if (!File.Exists(storagePath))
        {
            return null;
        }

        try
        {
            return await strategy.ExtractAsync(storagePath, cancellationToken);
        }
        catch
        {
            // Extraction must never fail the upload; the document is still stored and usable.
            return null;
        }
    }
}