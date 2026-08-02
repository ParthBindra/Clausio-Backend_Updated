using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Clausio.Legal.Infrastructure.Extraction;
using Clausio.Legal.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IDocumentService
{
    Task<List<Document>> ListAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<Document> UploadAsync(Guid caseId, string fileName, string? contentType, string? documentType, string? exhibitLabel, Stream content, long sizeBytes, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default);
}

public class DocumentService(ClausioDbContext db, IDocumentStorage storage, IDocumentTextExtractor textExtractor) : IDocumentService
{
    public Task<List<Document>> ListAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        db.Documents.AsNoTracking().Where(d => d.CaseId == caseId).OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Document> UploadAsync(Guid caseId, string fileName, string? contentType, string? documentType, string? exhibitLabel, Stream content, long sizeBytes, CancellationToken cancellationToken = default)
    {
        var documentId = Guid.NewGuid();
        var storagePath = await storage.SaveAsync(caseId, documentId, fileName, content, cancellationToken);
        var extractedText = await textExtractor.ExtractTextAsync(storagePath, fileName, cancellationToken);

        var entity = new Document
        {
            Id = documentId,
            CaseId = caseId,
            FileName = Path.GetFileName(fileName),
            DocumentType = documentType,
            ExhibitLabel = exhibitLabel,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            ExtractedText = extractedText,
        };

        db.Documents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid caseId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Documents.FirstOrDefaultAsync(d => d.CaseId == caseId && d.Id == id, cancellationToken);
        if (entity is null) return false;

        storage.Delete(entity.StoragePath);
        db.Documents.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}