using Cargo.API.DTOs;
using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для управления треками
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TracksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TracksController> _logger;

    public TracksController(IUnitOfWork unitOfWork, ILogger<TracksController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Получить все треки (текущего тенанта)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TrackDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tracks = await _unitOfWork.Tracks.GetAllAsync(cancellationToken);
        var trackDtos = tracks.Select(MapToDto);
        return Ok(trackDtos);
    }

    /// <summary>
    /// Получить трек по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
        
        if (track == null)
        {
            return NotFound(new { message = "Трек не найден" });
        }

        return Ok(MapToDto(track));
    }

    /// <summary>
    /// Получить трек по трек-номеру
    /// </summary>
    [HttpGet("by-tracking-number/{trackingNumber}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDto>> GetByTrackingNumber(
        string trackingNumber,
        CancellationToken cancellationToken)
    {
        var track = await _unitOfWork.Tracks.GetByTrackingNumberAsync(trackingNumber, cancellationToken);
        
        if (track == null)
        {
            return NotFound(new { message = "Трек не найден" });
        }

        return Ok(MapToDto(track));
    }

    /// <summary>
    /// Получить треки по коду клиента
    /// </summary>
    [HttpGet("by-client/{clientCode}")]
    [ProducesResponseType(typeof(IEnumerable<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TrackDto>>> GetByClientCode(
        string clientCode,
        CancellationToken cancellationToken)
    {
        var tracks = await _unitOfWork.Tracks.GetByClientCodeAsync(clientCode, cancellationToken);
        var trackDtos = tracks.Select(MapToDto);
        return Ok(trackDtos);
    }

    /// <summary>
    /// Получить треки по статусу
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TrackDto>>> GetByStatus(
        TrackStatus status,
        CancellationToken cancellationToken)
    {
        var tracks = await _unitOfWork.Tracks.GetByStatusAsync(status, cancellationToken);
        var trackDtos = tracks.Select(MapToDto);
        return Ok(trackDtos);
    }

    /// <summary>
    /// Создать новый трек
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackDto>> Create(
        [FromBody] CreateTrackDto dto,
        CancellationToken cancellationToken)
    {
        // Проверка на дубликат трек-номера
        var existingTrack = await _unitOfWork.Tracks.GetByTrackingNumberAsync(dto.TrackingNumber, cancellationToken);
        if (existingTrack != null)
        {
            return BadRequest(new { message = "Трек с таким номером уже существует" });
        }

        var track = new Track
        {
            ClientCode = dto.ClientCode,
            TrackingNumber = dto.TrackingNumber,
            Description = dto.Description,
            Weight = dto.Weight,
            DeclaredValue = dto.DeclaredValue,
            OriginCountry = dto.OriginCountry,
            DestinationCountry = dto.DestinationCountry,
            EstimatedDeliveryAt = dto.EstimatedDeliveryAt,
            Notes = dto.Notes,
            Status = TrackStatus.Created
        };

        await _unitOfWork.Tracks.AddAsync(track, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Создан новый трек: {TrackingNumber}", track.TrackingNumber);

        return CreatedAtAction(
            nameof(GetById),
            new { id = track.Id },
            MapToDto(track));
    }

    /// <summary>
    /// Обновить трек
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDto>> Update(
        Guid id,
        [FromBody] UpdateTrackDto dto,
        CancellationToken cancellationToken)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
        
        if (track == null)
        {
            return NotFound(new { message = "Трек не найден" });
        }

        if (dto.Description != null) track.Description = dto.Description;
        if (dto.Status.HasValue) track.Status = dto.Status.Value;
        if (dto.Weight.HasValue) track.Weight = dto.Weight;
        if (dto.DeclaredValue.HasValue) track.DeclaredValue = dto.DeclaredValue;
        if (dto.ShippedAt.HasValue) track.ShippedAt = dto.ShippedAt;
        if (dto.EstimatedDeliveryAt.HasValue) track.EstimatedDeliveryAt = dto.EstimatedDeliveryAt;
        if (dto.ActualDeliveryAt.HasValue) track.ActualDeliveryAt = dto.ActualDeliveryAt;
        if (dto.Notes != null) track.Notes = dto.Notes;

        await _unitOfWork.Tracks.UpdateAsync(track, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Обновлён трек: {TrackId}", track.Id);

        return Ok(MapToDto(track));
    }

    /// <summary>
    /// Удалить трек
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
        
        if (track == null)
        {
            return NotFound(new { message = "Трек не найден" });
        }

        await _unitOfWork.Tracks.DeleteAsync(track, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Удалён трек: {TrackId}", id);

        return NoContent();
    }

    private static TrackDto MapToDto(Track track)
    {
        return new TrackDto
        {
            Id = track.Id,
            ClientCode = track.ClientCode,
            TrackingNumber = track.TrackingNumber,
            Status = track.Status,
            Description = track.Description,
            Weight = track.Weight,
            DeclaredValue = track.DeclaredValue,
            OriginCountry = track.OriginCountry,
            DestinationCountry = track.DestinationCountry,
            ShippedAt = track.ShippedAt,
            EstimatedDeliveryAt = track.EstimatedDeliveryAt,
            ActualDeliveryAt = track.ActualDeliveryAt,
            Notes = track.Notes,
            CreatedAt = track.CreatedAt,
            UpdatedAt = track.UpdatedAt
        };
    }
}

