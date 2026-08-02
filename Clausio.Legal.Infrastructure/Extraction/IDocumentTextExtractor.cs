namespace Clausio.Legal.Infrastructure.Extraction;

public interface IDocumentTextExtractor
{
	Task<string?> ExtractTextAsync(string storagePath, string fileName, CancellationToken cancellationToken = default);
}