using System.Security.Claims;
using WebStok.Business.Interfaces;
using WebStok.Domain.Entities;

namespace WebStok.Web.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            if (Principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return int.TryParse(
                Principal.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId)
                ? userId
                : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            if (Principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var value = Principal.FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse<UserRole>(value, out var role) &&
                   Enum.IsDefined(typeof(UserRole), role)
                ? role
                : null;
        }
    }
}