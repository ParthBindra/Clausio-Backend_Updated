using System.Text;

namespace Clausio.Legal.Infrastructure.Extraction;

public class TxtExtractionStrategy : IDocumentTextExtractionStrategy
{
    public bool CanHandle(string fileExtension) =>
        string.Equals(fileExtension, ".txt", StringComparison.OrdinalIgnoreCase);

    public Task<string?> ExtractAsync(string storagePath, CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(storagePath, Encoding.UTF8, cancellationToken)!;
}