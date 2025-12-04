using Cargo.API.DTOs;
using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cargo.API.Controllers;

/// <summary>
/// Контроллер для управления тенантами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(IUnitOfWork unitOfWork, ILogger<TenantsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Получить всех тенантов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
        var tenantDtos = tenants.Select(MapToDto);
        return Ok(tenantDtos);
    }

    /// <summary>
    /// Получить тенанта по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        
        if (tenant == null)
        {
            return NotFound(new { message = "Тенант не найден" });
        }

        return Ok(MapToDto(tenant));
    }

    /// <summary>
    /// Получить тенанта по коду
    /// </summary>
    [HttpGet("by-code/{tenantCode}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetByCode(string tenantCode, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByTenantCodeAsync(tenantCode, cancellationToken);
        
        if (tenant == null)
        {
            return NotFound(new { message = "Тенант не найден" });
        }

        return Ok(MapToDto(tenant));
    }

    /// <summary>
    /// Получить активных тенантов
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetActive(CancellationToken cancellationToken)
    {
        var tenants = await _unitOfWork.Tenants.GetActivTenantsAsync(cancellationToken);
        var tenantDtos = tenants.Select(MapToDto);
        return Ok(tenantDtos);
    }

    /// <summary>
    /// Создать нового тенанта
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> Create(
        [FromBody] CreateTenantDto dto,
        CancellationToken cancellationToken)
    {
        // Проверка на дубликат кода
        var existingTenant = await _unitOfWork.Tenants.GetByTenantCodeAsync(dto.TenantCode, cancellationToken);
        if (existingTenant != null)
        {
            return BadRequest(new { message = "Тенант с таким кодом уже существует" });
        }

        var tenant = new Tenant
        {
            CompanyName = dto.CompanyName,
            TenantCode = dto.TenantCode,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            SubscriptionExpiresAt = dto.SubscriptionExpiresAt,
            IsActive = true
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Создан новый тенант: {TenantCode}", tenant.TenantCode);

        return CreatedAtAction(
            nameof(GetById),
            new { id = tenant.Id },
            MapToDto(tenant));
    }

    /// <summary>
    /// Обновить тенанта
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> Update(
        Guid id,
        [FromBody] UpdateTenantDto dto,
        CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        
        if (tenant == null)
        {
            return NotFound(new { message = "Тенант не найден" });
        }

        tenant.CompanyName = dto.CompanyName;
        tenant.ContactEmail = dto.ContactEmail;
        tenant.ContactPhone = dto.ContactPhone;
        tenant.IsActive = dto.IsActive;
        tenant.SubscriptionExpiresAt = dto.SubscriptionExpiresAt;

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Обновлён тенант: {TenantId}", tenant.Id);

        return Ok(MapToDto(tenant));
    }

    /// <summary>
    /// Удалить тенанта
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        
        if (tenant == null)
        {
            return NotFound(new { message = "Тенант не найден" });
        }

        await _unitOfWork.Tenants.DeleteAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Удалён тенант: {TenantId}", id);

        return NoContent();
    }

    private static TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            CompanyName = tenant.CompanyName,
            TenantCode = tenant.TenantCode,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            IsActive = tenant.IsActive,
            SubscriptionExpiresAt = tenant.SubscriptionExpiresAt,
            CreatedAt = tenant.CreatedAt
        };
    }
}


