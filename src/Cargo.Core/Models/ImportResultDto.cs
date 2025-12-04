namespace Cargo.Core.Models;

/// <summary>
/// DTO для результата импорта Excel файла
/// </summary>
public class ImportResultDto
{
    /// <summary>
    /// Общее количество обработанных строк
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Количество успешно импортированных строк
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество ошибок
    /// </summary>
    public int ErrorCount => Errors?.Count ?? 0;

    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<ImportErrorDto> Errors { get; set; } = new();

    /// <summary>
    /// Успешность импорта (все строки без ошибок)
    /// </summary>
    public bool IsSuccess => ErrorCount == 0 && SuccessCount > 0;
}

/// <summary>
/// DTO для ошибки импорта
/// </summary>
public class ImportErrorDto
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

