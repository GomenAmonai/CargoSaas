using Cargo.API.DTOs;
using Cargo.Core.Entities;
using Cargo.Core.Enums;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для работы треков текущего клиента (Telegram)
/// </summary>
[ApiController]
[Route("api/client/tracks")]
[Produces("application/json")]
[Authorize]
public class ClientTracksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ClientTracksController> _logger;

    public ClientTracksController(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger<ClientTracksController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Получить все треки текущего клиента
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TrackDto>>> GetMyTracks(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        if (user.Role != UserRole.Client)
        {
            // Для Manager/SystemAdmin используем общий контроллер TracksController
            _logger.LogWarning("Non-client user attempted to access client tracks: {UserId}, Role: {Role}", user.Id, user.Role);
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(user.ClientCode))
        {
            _logger.LogWarning("Client user has no ClientCode: {UserId}", user.Id);
            return Ok(Enumerable.Empty<TrackDto>());
        }

        var tracks = await _unitOfWork.Tracks.GetByClientCodeAsync(user.ClientCode, cancellationToken);
        var trackDtos = tracks.Select(MapToDto);
        return Ok(trackDtos);
    }

    /// <summary>
    /// Получить конкретный трек текущего клиента по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TrackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDto>> GetMyTrackById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        if (user.Role != UserRole.Client)
        {
            _logger.LogWarning("Non-client user attempted to access client track details: {UserId}, Role: {Role}", user.Id, user.Role);
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(user.ClientCode))
        {
            _logger.LogWarning("Client user has no ClientCode: {UserId}", user.Id);
            return NotFound(new { message = "Track not found" });
        }

        var track = await _unitOfWork.Tracks.GetByIdAsync(id, cancellationToken);
        if (track == null)
        {
            return NotFound(new { message = "Track not found" });
        }

        if (!string.Equals(track.ClientCode, user.ClientCode, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("User {UserId} attempted to access foreign track {TrackId}", user.Id, track.Id);
            return Forbid();
        }

        return Ok(MapToDto(track));
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

