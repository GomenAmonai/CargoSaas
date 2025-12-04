namespace Cargo.Core.Entities;

/// <summary>
/// Сущность трека (отслеживаемая посылка/груз)
/// </summary>
public class Track : BaseEntity
{
    /// <summary>
    /// Код клиента
    /// </summary>
    public string ClientCode { get; set; } = string.Empty;

    /// <summary>
    /// Трек-номер для отслеживания
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Статус трека
    /// </summary>
    public TrackStatus Status { get; set; } = TrackStatus.Created;

    /// <summary>
    /// Описание груза
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Вес груза (кг)
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Объявленная стоимость
    /// </summary>
    public decimal? DeclaredValue { get; set; }

    /// <summary>
    /// Страна отправления
    /// </summary>
    public string? OriginCountry { get; set; }

    /// <summary>
    /// Страна назначения
    /// </summary>
    public string? DestinationCountry { get; set; }

    /// <summary>
    /// Дата отправки
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Ожидаемая дата доставки
    /// </summary>
    public DateTime? EstimatedDeliveryAt { get; set; }

    /// <summary>
    /// Фактическая дата доставки
    /// </summary>
    public DateTime? ActualDeliveryAt { get; set; }

    /// <summary>
    /// Примечания
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Ссылка на тенанта
    /// </summary>
    public Tenant? Tenant { get; set; }
}

/// <summary>
/// Статусы трека
/// </summary>
public enum TrackStatus
{
    /// <summary>
    /// Создан
    /// </summary>
    Created = 0,

    /// <summary>
    /// Принят к отправке
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// В пути
    /// </summary>
    InTransit = 2,

    /// <summary>
    /// На таможне
    /// </summary>
    CustomsProcessing = 3,

    /// <summary>
    /// Прибыл в страну назначения
    /// </summary>
    ArrivedAtDestination = 4,

    /// <summary>
    /// Отправлен для доставки
    /// </summary>
    OutForDelivery = 5,

    /// <summary>
    /// Доставлен
    /// </summary>
    Delivered = 6,

    /// <summary>
    /// Задержан
    /// </summary>
    Delayed = 7,

    /// <summary>
    /// Возвращён отправителю
    /// </summary>
    ReturnedToSender = 8,

    /// <summary>
    /// Отменён
    /// </summary>
    Cancelled = 9
}

