/* File: PendingActivationsController.cs | Author: Dilnuk De Silva | Created: 27/09/2026 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice)]
public class PendingActivationsController : Controller
{
    private readonly ApiClient _api;
    public PendingActivationsController(ApiClient api) { _api = api; }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try { return View(await _api.GetPendingProsumersAsync(cancellationToken)); }
        catch (ApiClientException ex) { TempData["Error"] = ex.ApiMessage; return View(Array.Empty<UserDto>()); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Activate(string id, CancellationToken cancellationToken) =>
        Run(() => _api.ActivateProsumerAsync(id, cancellationToken), $"Activated {id}.");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(string id, CancellationToken cancellationToken) =>
        Run(() => _api.DeactivateProsumerAsync(id, cancellationToken), $"Rejected {id}.");

    private async Task<IActionResult> Run(Func<Task<UserDto>> command, string success)
    {
        try { await command(); TempData["Success"] = success; }
        catch (ApiClientException ex) { TempData["Error"] = ex.ApiMessage; }
        return RedirectToAction(nameof(Index));
    }
}
