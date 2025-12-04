using Cargo.Core.Interfaces;
using Cargo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cargo.Infrastructure.Repositories;

/// <summary>
/// Unit of Work для управления транзакциями и доступом к репозиториям
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CargoDbContext _context;
    private IDbContextTransaction? _transaction;

    private ITrackRepository? _trackRepository;
    private ITenantRepository? _tenantRepository;

    public UnitOfWork(CargoDbContext context)
    {
        _context = context;
    }

    public ITrackRepository Tracks
    {
        get
        {
            _trackRepository ??= new TrackRepository(_context);
            return _trackRepository;
        }
    }

    public ITenantRepository Tenants
    {
        get
        {
            _tenantRepository ??= new TenantRepository(_context);
            return _tenantRepository;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}


