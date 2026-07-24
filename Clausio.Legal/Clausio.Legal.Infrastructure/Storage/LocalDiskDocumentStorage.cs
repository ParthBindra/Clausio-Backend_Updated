namespace Clausio.Legal.Infrastructure.Storage;

public class LocalDiskDocumentStorage(string rootPath) : IDocumentStorage
{
    public async Task<string> SaveAsync(Guid caseId, Guid documentId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var caseDir = Path.Combine(rootPath, caseId.ToString());
        Directory.CreateDirectory(caseDir);

        var storagePath = Path.Combine(caseDir, $"{documentId}_{safeFileName}");
        await using var fileStream = File.Create(storagePath);
        await content.CopyToAsync(fileStream, cancellationToken);
        return storagePath;
    }

    public void Delete(string storagePath)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
        }
    }
}
