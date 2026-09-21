using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Web.Security;

namespace WebStok.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        LoginDto input,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            try
            {
                var session = await _authService.LoginAsync(
                    input, cancellationToken);

                if (session is not null)
                {
                    var properties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = false,
                        ExpiresUtc = new DateTimeOffset(
                            session.ExpiresAtUtc)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        WebStokCookieEvents.CreatePrincipal(session),
                        properties);

                    if (!string.IsNullOrEmpty(returnUrl) &&
                        Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Kullanıcı adı veya şifre hatalı.");
            }
            catch (ValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }
            }
        }

        input.Password = string.Empty;
        return View(input);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(
                User.FindFirstValue("session_id"), out var sessionId) &&
            int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            await _authService.LogoutAsync(
                sessionId, userId, cancellationToken);
        }

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}