using Cargo.API.DTOs;
using Cargo.Core.Models;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для импорта данных
/// </summary>
[ApiController]
[Route("api/tracks/[controller]")]
[Produces("application/json")]
public class ImportController : ControllerBase
{
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(IExcelImportService excelImportService, ILogger<ImportController> logger)
    {
        _excelImportService = excelImportService;
        _logger = logger;
    }

    /// <summary>
    /// Импортировать треки из Excel файла
    /// </summary>
    /// <param name="file">Excel файл (.xlsx)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат импорта</returns>
    /// <remarks>
    /// Формат Excel файла:
    /// - Строка 1: Заголовки (игнорируется)
    /// - Колонка A: TrackingNumber (обязательно)
    /// - Колонка B: ClientCode (обязательно)
    /// - Колонка C: Weight (опционально, число)
    /// - Колонка D: Description (опционально)
    /// - Колонка E: Status (опционально, enum: Created, InTransit, Delivered и т.д.)
    /// 
    /// Если трек с таким TrackingNumber уже существует - будет обновлен.
    /// Если нет - будет создан новый.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10_485_760)] // 10 MB максимум
    public async Task<ActionResult<ImportResultDto>> ImportFromExcel(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        // Валидация файла
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не предоставлен или пустой" });
        }

        // Проверка расширения
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "Поддерживаются только Excel файлы (.xlsx, .xls)" });
        }

        // Проверка размера (максимум 10 MB)
        if (file.Length > 10_485_760)
        {
            return BadRequest(new { message = "Размер файла превышает 10 MB" });
        }

        _logger.LogInformation(
            "Начало импорта из файла: {FileName}, размер: {Size} байт",
            file.FileName,
            file.Length);

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _excelImportService.ImportTracksAsync(stream, cancellationToken);

            _logger.LogInformation(
                "Импорт завершен: {Total} обработано, {Success} успешно, {Errors} ошибок",
                result.TotalProcessed,
                result.SuccessCount,
                result.Errors.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте файла {FileName}", file.FileName);
            return BadRequest(new { message = $"Ошибка импорта: {ex.Message}" });
        }
    }

    /// <summary>
    /// Скачать шаблон Excel файла для импорта
    /// </summary>
    /// <returns>Excel файл шаблон</returns>
    [HttpGet("template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DownloadTemplate()
    {
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Tracks");

        // Заголовки
        worksheet.Cells[1, 1].Value = "TrackingNumber";
        worksheet.Cells[1, 2].Value = "ClientCode";
        worksheet.Cells[1, 3].Value = "Weight";
        worksheet.Cells[1, 4].Value = "Description";
        worksheet.Cells[1, 5].Value = "Status";

        // Пример данных
        worksheet.Cells[2, 1].Value = "TRACK001";
        worksheet.Cells[2, 2].Value = "CLIENT001";
        worksheet.Cells[2, 3].Value = 5.5;
        worksheet.Cells[2, 4].Value = "Электроника из Китая";
        worksheet.Cells[2, 5].Value = "Created";

        worksheet.Cells[3, 1].Value = "TRACK002";
        worksheet.Cells[3, 2].Value = "CLIENT002";
        worksheet.Cells[3, 3].Value = 12.3;
        worksheet.Cells[3, 4].Value = "Текстиль из Турции";
        worksheet.Cells[3, 5].Value = "InTransit";

        // Форматирование
        worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
        worksheet.Cells.AutoFitColumns();

        var fileBytes = package.GetAsByteArray();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "tracks_import_template.xlsx");
    }
}

