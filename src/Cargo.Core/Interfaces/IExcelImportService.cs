using Cargo.Core.Models;

namespace Cargo.Core.Interfaces;

/// <summary>
/// Сервис для импорта треков из Excel файлов
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Импортировать треки из Excel файла
    /// </summary>
    /// <param name="fileStream">Поток Excel файла</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат импорта</returns>
    Task<ImportResultDto> ImportTracksAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
