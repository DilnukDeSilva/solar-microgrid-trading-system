/*
 * File: AccountController.cs
 * Description: Login, logout and 403 pages. Credentials are checked only by the API.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;
using SmartSolar.Web.Services;

namespace SmartSolar.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebSession _session;

    // Injects the shared API client and the cookie/session helper.
    public AccountController(ApiClient api, IWebSession session)
    {
        _api = api;
        _session = session;
    }

    // Shows the login form. Already-signed-in users go to their role home.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(bool timedOut = false, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }

        if (timedOut)
        {
            ModelState.AddModelError(string.Empty, "Your session expired. Please sign in again.");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    // Posts username and password to POST /api/auth/login, then stores the JWT in session.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _api.LoginAsync(model.Username.Trim(), model.Password, cancellationToken);
            await _session.SignInAsync(result);
            return RedirectAfterLogin(result.User.Role, returnUrl);
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
            return View(model);
        }
    }

    // Clears the JWT session and the auth cookie.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _session.SignOutAsync();
        return RedirectToAction("Login");
    }

    // Shown when the signed-in role cannot open the requested page.
    [Authorize]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Honours a local returnUrl, otherwise sends the user to the role dashboard.
    private IActionResult RedirectAfterLogin(string role, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (string.Equals(role, RoleNames.Backoffice, StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (string.Equals(role, RoleNames.GridOperator, StringComparison.Ordinal))
        {
            return RedirectToAction("Index", "Operations");
        }

        return RedirectToAction(nameof(AccessDenied));
    }

    // Sends Backoffice to Admin and GridOperator to Operations when already signed in.
    private IActionResult RedirectToRoleHome()
    {
        if (User.IsInRole(RoleNames.Backoffice))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (User.IsInRole(RoleNames.GridOperator))
        {
            return RedirectToAction("Index", "Operations");
        }

        return RedirectToAction(nameof(AccessDenied));
    }
}
