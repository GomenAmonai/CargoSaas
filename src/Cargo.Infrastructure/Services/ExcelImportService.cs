using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Cargo.Core.Models;
using Cargo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace Cargo.Infrastructure.Services;

/// <summary>
/// Сервис для импорта треков из Excel файлов
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly CargoDbContext _context;
    private readonly ILogger<ExcelImportService> _logger;

    public ExcelImportService(CargoDbContext context, ILogger<ExcelImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportTracksAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();

        try
        {
            // Устанавливаем лицензию EPPlus для некоммерческого использования
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = 0,
                    ErrorMessage = "Excel файл не содержит листов"
                });
                return result;
            }

            // Получаем количество строк
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount <= 1)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = 0,
                    ErrorMessage = "Excel файл пустой или содержит только заголовки"
                });
                return result;
            }

            _logger.LogInformation("Начало импорта треков. Всего строк: {RowCount}", rowCount - 1);

            // Обрабатываем строки (пропускаем первую строку с заголовками)
            for (int row = 2; row <= rowCount; row++)
            {
                result.TotalProcessed++;

                try
                {
                    // Читаем данные из колонок
                    var trackingNumber = worksheet.Cells[row, 1].Text?.Trim(); // A - TrackingNumber
                    var clientCode = worksheet.Cells[row, 2].Text?.Trim();     // B - ClientCode
                    var weightText = worksheet.Cells[row, 3].Text?.Trim();     // C - Weight
                    var description = worksheet.Cells[row, 4].Text?.Trim();    // D - Description
                    var statusText = worksheet.Cells[row, 5].Text?.Trim();     // E - Status

                    // Валидация обязательных полей
                    if (string.IsNullOrEmpty(trackingNumber))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row,
                            ErrorMessage = "TrackingNumber обязателен",
                            TrackingNumber = trackingNumber
                        });
                        continue;
                    }

                    if (string.IsNullOrEmpty(clientCode))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row,
                            ErrorMessage = "ClientCode обязателен",
                            TrackingNumber = trackingNumber
                        });
                        continue;
                    }

                    // Парсинг Weight
                    decimal? weight = null;
                    if (!string.IsNullOrEmpty(weightText) && decimal.TryParse(weightText, out var parsedWeight))
                    {
                        weight = parsedWeight;
                    }

                    // Парсинг Status
                    var status = TrackStatus.Created;
                    if (!string.IsNullOrEmpty(statusText) && Enum.TryParse<TrackStatus>(statusText, true, out var parsedStatus))
                    {
                        status = parsedStatus;
                    }

                    // Upsert логика: проверяем существует ли трек
                    var existingTrack = await _context.Tracks
                        .FirstOrDefaultAsync(t => t.TrackingNumber == trackingNumber, cancellationToken);

                    if (existingTrack != null)
                    {
                        // Обновляем существующий трек
                        existingTrack.Status = status;
                        existingTrack.Weight = weight;
                        existingTrack.Description = description;
                        existingTrack.UpdatedAt = DateTime.UtcNow;

                        _context.Tracks.Update(existingTrack);
                        _logger.LogDebug("Обновлен трек: {TrackingNumber}", trackingNumber);
                    }
                    else
                    {
                        // Создаем новый трек
                        var newTrack = new Track
                        {
                            TrackingNumber = trackingNumber,
                            ClientCode = clientCode,
                            Weight = weight,
                            Description = description,
                            Status = status
                        };

                        await _context.Tracks.AddAsync(newTrack, cancellationToken);
                        _logger.LogDebug("Создан новый трек: {TrackingNumber}", trackingNumber);
                    }

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка обработки строки {Row}", row);
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = row,
                        ErrorMessage = $"Ошибка обработки: {ex.Message}"
                    });
                }
            }

            // Сохраняем все изменения одной транзакцией
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Импорт завершен. Обработано: {Total}, Успешно: {Success}, Ошибок: {Errors}",
                result.TotalProcessed, result.SuccessCount, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при импорте Excel файла");
            result.Errors.Add(new ImportErrorDto
            {
                RowNumber = 0,
                ErrorMessage = $"Критическая ошибка: {ex.Message}"
            });
        }

        return result;
    }
}
