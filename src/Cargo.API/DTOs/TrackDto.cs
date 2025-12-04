using Cargo.Core.Entities;

namespace Cargo.API.DTOs;

/// <summary>
/// DTO для трека
/// </summary>
public class TrackDto
{
    public Guid Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public TrackStatus Status { get; set; }
    public string? Description { get; set; }
    public decimal? Weight { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? ActualDeliveryAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO для создания трека
/// </summary>
public class CreateTrackDto
{
    public string ClientCode { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Weight { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO для обновления трека
/// </summary>
public class UpdateTrackDto
{
    public string? Description { get; set; }
    public TrackStatus? Status { get; set; }
    public decimal? Weight { get; set; }
    public decimal? DeclaredValue { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDeliveryAt { get; set; }
    public DateTime? ActualDeliveryAt { get; set; }
    public string? Notes { get; set; }
}

