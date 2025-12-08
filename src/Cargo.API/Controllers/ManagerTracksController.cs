using Cargo.API.DTOs;
using Cargo.Core;
using Cargo.Core.Entities;
using Cargo.Core.Exceptions;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для управления треками (CRUD) для менеджеров
/// </summary>
[ApiController]
[Route("api/manager/tracks")]
[Authorize(Roles = "Manager,SystemAdmin")]
[Produces("application/json")]
public class ManagerTracksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManagerTracksController> _logger;

    public ManagerTracksController(
        IUnitOfWork unitOfWork,
        ILogger<ManagerTracksController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех треков с фильтрацией
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrackDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TrackDto>>> GetTracks(
        [FromQuery] string? search,
        [FromQuery] string? clientCode,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            // Получаем все треки (автоматическая фильтрация по TenantId через Query Filter)
            var tracks = await _unitOfWork.Tracks.GetAllAsync(cancellationToken);

            // Клиентская фильтрация (для MVP достаточно)
            var query = tracks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLowerInvariant();
                query = query.Where(t => 
                    t.TrackingNumber.ToLowerInvariant().Contains(search) ||
                    (t.ClientCode != null && t.ClientCode.ToLowerInvariant().Contains(search)) ||
                    (t.Description != null && t.Description.ToLowerInvariant().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(clientCode))
            {
                query = query.Where(t => t.ClientCode == clientCode);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TrackStatus>(status, out var trackStatus))
            {
                query = query.Where(t => t.Status == trackStatus);
            }

            // Сортировка по дате создания (новые первые)
            var result = query
                .OrderByDescending(t => t.CreatedAt)
                .Select(MapToDto)
                .ToList();

            _logger.LogInformation("Retrieved {Count} tracks (filtered)", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tracks");
            throw new BusinessException("Failed to retrieve tracks");
        }
    }

    /// <summary>
    /// Получить трек по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDto>> GetTrackById(Guid id, CancellationToken cancellationToken)
    {
        var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
        
        if (track == null)
        {
            throw new NotFoundException("Track", id);
        }

        return Ok(MapToDto(track));
    }

    /// <summary>
    /// Создать новый трек
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackDto>> CreateTrack(
        [FromBody] CreateTrackDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Проверяем уникальность TrackingNumber в рамках тенанта
            var existingTracks = await _unitOfWork.Tracks.GetAllAsync(cancellationToken);
            if (existingTracks.Any(t => t.TrackingNumber == request.TrackingNumber))
            {
                throw new ConflictException("Track", "TrackingNumber", request.TrackingNumber);
            }

            var track = new Track
            {
                TrackingNumber = request.TrackingNumber,
                ClientCode = request.ClientCode,
                Status = request.Status,
                Description = request.Description,
                Weight = request.Weight,
                DeclaredValue = request.DeclaredValue,
                OriginCountry = request.OriginCountry,
                DestinationCountry = request.DestinationCountry,
                ShippedAt = request.ShippedAt,
                EstimatedDeliveryAt = request.EstimatedDeliveryAt,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tracks.AddAsync(track, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new track: {TrackId}, TrackingNumber={TrackingNumber}", 
                track.Id, track.TrackingNumber);

            return CreatedAtAction(
                nameof(GetTrackById), 
                new { id = track.Id }, 
                MapToDto(track));
        }
        catch (CargoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating track");
            throw new BusinessException("Failed to create track");
        }
    }

    /// <summary>
    /// Обновить существующий трек
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackDto>> UpdateTrack(
        Guid id,
        [FromBody] UpdateTrackDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
            if (track == null)
            {
                throw new NotFoundException("Track", id);
            }

            // Проверяем уникальность TrackingNumber если он изменился
            if (track.TrackingNumber != request.TrackingNumber)
            {
                var existingTracks = await _unitOfWork.Tracks.GetAllAsync(cancellationToken);
                if (existingTracks.Any(t => t.TrackingNumber == request.TrackingNumber && t.Id != id))
                {
                    throw new ConflictException("Track", "TrackingNumber", request.TrackingNumber);
                }
            }

            // Обновляем поля
            track.TrackingNumber = request.TrackingNumber;
            track.ClientCode = request.ClientCode;
            if (request.Status.HasValue)
            {
                track.Status = request.Status.Value;
            }
            if (request.Description != null) track.Description = request.Description;
            track.Weight = request.Weight;
            track.DeclaredValue = request.DeclaredValue;
            if (request.OriginCountry != null) track.OriginCountry = request.OriginCountry;
            if (request.DestinationCountry != null) track.DestinationCountry = request.DestinationCountry;
            track.ShippedAt = request.ShippedAt;
            track.EstimatedDeliveryAt = request.EstimatedDeliveryAt;
            track.ActualDeliveryAt = request.ActualDeliveryAt;
            if (request.Notes != null) track.Notes = request.Notes;
            track.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Tracks.UpdateAsync(track, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated track: {TrackId}, TrackingNumber={TrackingNumber}", 
                track.Id, track.TrackingNumber);

            return Ok(MapToDto(track));
        }
        catch (CargoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating track {TrackId}", id);
            throw new BusinessException("Failed to update track");
        }
    }

    /// <summary>
    /// Удалить трек
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTrack(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
            if (track == null)
            {
                throw new NotFoundException("Track", id);
            }

            await _unitOfWork.Tracks.DeleteAsync(track, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted track: {TrackId}, TrackingNumber={TrackingNumber}", 
                track.Id, track.TrackingNumber);

            return NoContent();
        }
        catch (CargoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting track {TrackId}", id);
            throw new BusinessException("Failed to delete track");
        }
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
