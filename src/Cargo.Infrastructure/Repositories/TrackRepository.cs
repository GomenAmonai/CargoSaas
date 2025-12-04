using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cargo.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с треками
/// </summary>
public class TrackRepository : Repository<Track>, ITrackRepository
{
    public TrackRepository(CargoDbContext context) : base(context)
    {
    }

    public async Task<Track?> GetByTrackingNumberAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Tenant)
            .FirstOrDefaultAsync(t => t.TrackingNumber == trackingNumber, cancellationToken);
    }

    public async Task<IEnumerable<Track>> GetByClientCodeAsync(
        string clientCode,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Tenant)
            .Where(t => t.ClientCode == clientCode)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Track>> GetByStatusAsync(
        TrackStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Tenant)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}


