/*
 * File: WebSession.cs
 * Description: Stores the API JWT in session and mirrors role claims onto the auth cookie.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Services;

public class WebSession : IWebSession
{
    public const string JwtSessionKey = "JwtToken";

    private readonly IHttpContextAccessor _httpContextAccessor;

    // Injects the current HTTP context used to read and write session and cookies.
    public WebSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Writes the JWT to session and signs the auth cookie from the login payload.
    public async Task SignInAsync(LoginResponseDto login)
    {
        var context = RequireHttpContext();
        context.Session.SetString(JwtSessionKey, login.Token);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, login.User.Id),
            new(ClaimTypes.Name, login.User.Username),
            new(ClaimTypes.Role, login.User.Role),
            new("FullName", login.User.FullName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    // Clears the JWT session key and the auth cookie.
    public async Task SignOutAsync()
    {
        var context = RequireHttpContext();
        context.Session.Remove(JwtSessionKey);
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    // Returns the bearer token used by ApiClient, or null when the user is not signed in.
    public string? GetJwt()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(JwtSessionKey);
    }

    // Returns the current HTTP context or throws when the accessor is empty.
    private HttpContext RequireHttpContext()
    {
        return _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available.");
    }
}
