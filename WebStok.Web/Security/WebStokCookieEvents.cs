using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;

namespace WebStok.Web.Security;

public class WebStokCookieEvents : CookieAuthenticationEvents
{
    private readonly IAuthService _authService;

    public WebStokCookieEvents(IAuthService authService)
    {
        _authService = authService;
    }

    public static ClaimsPrincipal CreatePrincipal(AuthSessionDto session)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                session.UserId.ToString()),

            new Claim(ClaimTypes.Name, session.DisplayName),

            new Claim(ClaimTypes.Role, session.Role.ToString()),

            new Claim("session_id", session.SessionId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var sessionValue =
            context.Principal?.FindFirstValue("session_id");

        var userValue =
            context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(sessionValue, out var sessionId) &&
            int.TryParse(userValue, out var userId))
        {
            var session = await _authService.GetSessionAsync(
                sessionId,
                context.HttpContext.RequestAborted);

            if (session is not null && session.UserId == userId)
            {
                context.ReplacePrincipal(CreatePrincipal(session));
                return;
            }
        }

        context.RejectPrincipal();

        await context.HttpContext.SignOutAsync(
            context.Scheme.Name);
    }
}