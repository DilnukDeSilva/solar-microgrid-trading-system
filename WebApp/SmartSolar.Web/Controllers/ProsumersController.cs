/*
 * File: ProsumersController.cs
 * Description: Staff MVC pages for prosumer search and Backoffice account management.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
public class ProsumersController : Controller
{
    private readonly ApiClient _api;
    public ProsumersController(ApiClient api) { _api = api; }

    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? q, CancellationToken cancellationToken)
    {
        ViewBag.Status = status;
        ViewBag.Query = q;
        try { return View(await _api.GetProsumersAsync(status, q, cancellationToken)); }
        catch (ApiClientException ex) { TempData["Error"] = ex.ApiMessage; return View(Array.Empty<UserDto>()); }
    }

    [HttpGet, Authorize(Roles = RoleNames.Backoffice)]
    public IActionResult Create() => View(new ProsumerForm());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public async Task<IActionResult> Create(ProsumerForm form, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Password)) ModelState.AddModelError(nameof(form.Password), "Password is required.");
        if (!ModelState.IsValid) return View(form);
        try {
            await _api.CreateProsumerAsync(form, cancellationToken);
            TempData["Success"] = $"Created active prosumer {form.Nic}.";
            return RedirectToAction(nameof(Index));
        } catch (ApiClientException ex) { ModelState.AddModelError(string.Empty, ex.ApiMessage); return View(form); }
    }

    [HttpGet, Authorize(Roles = RoleNames.Backoffice)]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        try {
            var user = await _api.GetProsumerAsync(id, cancellationToken);
            return View(new ProsumerForm { Nic = user.Nic ?? user.Id, Username = user.Username, FullName = user.FullName, Email = user.Email, Phone = user.Phone, IsEdit = true });
        } catch (ApiClientException ex) { TempData["Error"] = ex.ApiMessage; return RedirectToAction(nameof(Index)); }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public async Task<IActionResult> Edit(string id, ProsumerForm form, CancellationToken cancellationToken)
    {
        form.Nic = id; form.IsEdit = true; ModelState.Remove(nameof(ProsumerForm.Nic));
        if (!ModelState.IsValid) return View(form);
        try {
            await _api.UpdateProsumerAsync(id, form, cancellationToken);
            TempData["Success"] = $"Updated {id}.";
            return RedirectToAction(nameof(Index));
        } catch (ApiClientException ex) { ModelState.AddModelError(string.Empty, ex.ApiMessage); return View(form); }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken) =>
        RunStatusAction(() => _api.DeactivateProsumerAsync(id, cancellationToken), $"Deactivated {id}.");

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public Task<IActionResult> Reactivate(string id, CancellationToken cancellationToken) =>
        RunStatusAction(() => _api.ReactivateProsumerAsync(id, cancellationToken), $"Reactivated {id}.");

    private async Task<IActionResult> RunStatusAction(Func<Task<UserDto>> command, string success)
    {
        try { await command(); TempData["Success"] = success; }
        catch (ApiClientException ex) { TempData["Error"] = ex.ApiMessage; }
        return RedirectToAction(nameof(Index));
    }
}
