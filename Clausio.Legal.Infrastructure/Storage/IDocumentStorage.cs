namespace Clausio.Legal.Infrastructure.Storage;

public interface IDocumentStorage
{
    Task<string> SaveAsync(Guid caseId, Guid documentId, string fileName, Stream content, CancellationToken cancellationToken = default);
    void Delete(string storagePath);
}
