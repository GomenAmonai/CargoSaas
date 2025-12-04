using Cargo.Core.Entities;
using System.Linq.Expressions;

namespace Cargo.Core.Interfaces;

/// <summary>
/// Базовый интерфейс репозитория для работы с сущностями
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Получить сущность по идентификатору
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все сущности
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Найти сущности по условию
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую сущность
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую сущность
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сущность
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование сущности по условию
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить количество сущностей
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}


