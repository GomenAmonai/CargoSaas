using Cargo.Core.Entities;

namespace Cargo.Core.Interfaces;

/// <summary>
/// Репозиторий для работы с треками
/// </summary>
public interface ITrackRepository : IRepository<Track>
{
    /// <summary>
    /// Получить трек по трек-номеру
    /// </summary>
    Task<Track?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить треки по коду клиента
    /// </summary>
    Task<IEnumerable<Track>> GetByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить треки по статусу
    /// </summary>
    Task<IEnumerable<Track>> GetByStatusAsync(TrackStatus status, CancellationToken cancellationToken = default);
}

