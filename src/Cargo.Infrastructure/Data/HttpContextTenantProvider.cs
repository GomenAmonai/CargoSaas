using System.Security.Claims;
using Cargo.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cargo.Infrastructure.Data;

/// <summary>
/// Провайдер тенанта, извлекающий TenantId из JWT claims в HttpContext
/// </summary>
public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpContextTenantProvider> _logger;

    public HttpContextTenantProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpContextTenantProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Guid GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Если нет HttpContext (например, фоновые задачи)
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null. Returning empty TenantId");
            return Guid.Empty;
        }

        // Проверяем аутентификацию
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("User is not authenticated. Returning empty TenantId");
            return Guid.Empty;
        }

        // Извлекаем tenantId claim
        var tenantIdClaim = httpContext.User.FindFirst(AppConstants.Jwt.TenantIdClaimType);
        
        if (tenantIdClaim == null)
        {
            _logger.LogWarning("TenantId claim not found for authenticated user: {UserId}", 
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Guid.Empty;
        }

        // Парсим Guid
        if (Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            _logger.LogDebug("TenantId resolved from claims: {TenantId}", tenantId);
            return tenantId;
        }

        _logger.LogError("Failed to parse TenantId from claim: {ClaimValue}", tenantIdClaim.Value);
        return Guid.Empty;
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        // HttpContext провайдер не поддерживает установку TenantId
        // TenantId определяется только из JWT токена
        _logger.LogWarning("SetCurrentTenantId called on HttpContextTenantProvider. This operation is not supported.");
    }
}

