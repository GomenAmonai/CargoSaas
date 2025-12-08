using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для получения статистики для dashboard менеджера
/// </summary>
[ApiController]
[Route("api/manager/statistics")]
[Authorize(Roles = "Manager,SystemAdmin")]
[Produces("application/json")]
public class ManagerStatisticsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManagerStatisticsController> _logger;

    public ManagerStatisticsController(
        IUnitOfWork unitOfWork,
        ILogger<ManagerStatisticsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Получить сводную статистику для dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics(
        CancellationToken cancellationToken)
    {
        try
        {
            // Получаем все треки (автоматическая фильтрация по TenantId)
            var tracks = await _unitOfWork.Tracks.GetAllAsync(cancellationToken);
            var tracksList = tracks.ToList();

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);

            var statistics = new DashboardStatisticsDto
            {
                // Общие метрики
                TotalTracks = tracksList.Count,
                ActiveClients = tracksList
                    .Where(t => !string.IsNullOrEmpty(t.ClientCode))
                    .Select(t => t.ClientCode)
                    .Distinct()
                    .Count(),

                // Треки по статусам
                TracksByStatus = tracksList
                    .GroupBy(t => t.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),

                // Треки в процессе
                TracksInTransit = tracksList.Count(t => 
                    t.Status == TrackStatus.InTransit || 
                    t.Status == TrackStatus.CustomsProcessing || 
                    t.Status == TrackStatus.OutForDelivery),

                // Доставлено за неделю
                TracksDeliveredThisWeek = tracksList.Count(t => 
                    t.Status == TrackStatus.Delivered && 
                    t.ActualDeliveryAt.HasValue && 
                    t.ActualDeliveryAt.Value >= weekStart),

                // Создано сегодня
                TracksCreatedToday = tracksList.Count(t => t.CreatedAt.Date == todayStart),

                // Задержанные треки
                DelayedTracks = tracksList.Count(t => 
                    t.Status != TrackStatus.Delivered && 
                    t.Status != TrackStatus.Cancelled &&
                    t.EstimatedDeliveryAt.HasValue && 
                    t.EstimatedDeliveryAt.Value < now),

                // Последние треки (топ 5)
                RecentTracks = tracksList
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new RecentTrackDto
                    {
                        Id = t.Id,
                        TrackingNumber = t.TrackingNumber,
                        ClientCode = t.ClientCode,
                        Status = t.Status.ToString(),
                        CreatedAt = t.CreatedAt
                    })
                    .ToList()
            };

            _logger.LogInformation("Dashboard statistics retrieved: Total={Total}, InTransit={InTransit}", 
                statistics.TotalTracks, statistics.TracksInTransit);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
            return StatusCode(500, new { message = "Failed to retrieve statistics" });
        }
    }
}

/// <summary>
/// DTO для статистики dashboard
/// </summary>
public class DashboardStatisticsDto
{
    public int TotalTracks { get; set; }
    public int ActiveClients { get; set; }
    public Dictionary<string, int> TracksByStatus { get; set; } = new();
    public int TracksInTransit { get; set; }
    public int TracksDeliveredThisWeek { get; set; }
    public int TracksCreatedToday { get; set; }
    public int DelayedTracks { get; set; }
    public List<RecentTrackDto> RecentTracks { get; set; } = new();
}

/// <summary>
/// DTO для последних треков
/// </summary>
public class RecentTrackDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
