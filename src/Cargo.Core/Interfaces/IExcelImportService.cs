namespace Cargo.Core.Interfaces;

/// <summary>
/// Сервис для импорта данных из Excel
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

/// <summary>
/// DTO с результатами импорта
/// </summary>
public class ImportResultDto
{
    /// <summary>
    /// Всего обработано строк
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Успешно импортировано
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<ImportError> Errors { get; set; } = new();
}

/// <summary>
/// Информация об ошибке импорта
/// </summary>
public class ImportError
{
    /// <summary>
    /// Номер строки в Excel файле
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Трек-номер (если был указан)
    /// </summary>
    public string? TrackingNumber { get; set; }
}

