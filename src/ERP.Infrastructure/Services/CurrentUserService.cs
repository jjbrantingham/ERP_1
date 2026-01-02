using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Implementation of current user service using HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?
                    .FindFirst("sub")
                ?? _httpContextAccessor.HttpContext?.User?
                    .FindFirst("UserId");

            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public string? Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Name)?.Value
                ?? _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.Email)?.Value
                ?? _httpContextAccessor.HttpContext?.User?
                    .FindFirst("preferred_username")?.Value;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
