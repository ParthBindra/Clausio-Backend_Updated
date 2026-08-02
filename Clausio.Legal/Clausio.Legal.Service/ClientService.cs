using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Core.Entities;
using Clausio.Legal.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IClientService
{
    Task<List<Client>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Client?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client> CreateAsync(CreateClientDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Client?> UpdateAsync(Guid id, CreateClientDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ClientService(ClausioDbContext db) : IClientService
{
    // ✅ FIXED — only returns clients belonging to the logged-in user
    public Task<List<Client>> ListAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Clients
            .AsNoTracking()
            .Where(c => c.CreatedByUserId == userId)
            .OrderBy(c => c.LastName)
            .ToListAsync(cancellationToken);

    public Task<Client?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    // ✅ FIXED — saves createdByUserId when creating client
    public async Task<Client> CreateAsync(CreateClientDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var client = Map(new Client(), dto);
        client.CreatedByUserId = createdByUserId;
        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<Client?> UpdateAsync(Guid id, CreateClientDto dto, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null) return null;

        Map(client, dto);
        await db.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null) return false;

        db.Clients.Remove(client);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Client Map(Client client, CreateClientDto dto)
    {
        client.FirstName     = dto.FirstName;
        client.LastName      = dto.LastName;
        client.Phone         = dto.Phone;
        client.AltPhone      = dto.AltPhone;
        client.Email         = dto.Email;
        client.WhatsApp      = dto.WhatsApp;
        client.Address       = dto.Address;
        client.ClientType    = dto.ClientType;
        client.Aadhar        = dto.Aadhar;
        client.Pan           = dto.Pan;
        client.Occupation    = dto.Occupation;
        client.MonthlyIncome = dto.MonthlyIncome;
        client.BankName      = dto.BankName;
        client.IsVip         = dto.IsVip;
        client.Notes         = dto.Notes;
        return client;
    }
}
