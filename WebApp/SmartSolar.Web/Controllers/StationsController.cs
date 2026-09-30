/*
 * File: StationsController.cs
 * Description: Staff MVC pages for the station list. Status changes go through the API.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
public class StationsController : Controller
{
    private readonly ApiClient _api;

    // Injects the API client that attaches the session JWT.
    public StationsController(ApiClient api)
    {
        _api = api;
    }

    // Lists stations. Operators can view; they cannot change status.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetStationsAsync(cancellationToken));
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
            return View(Array.Empty<StationDto>());
        }
    }

    // Deactivates a station. The API's 409 is shown as a banner.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken) =>
        RunStatusAction(() => _api.DeactivateStationAsync(id, cancellationToken), $"Deactivated {id}.");

    // Activates a station again.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.Backoffice)]
    public Task<IActionResult> Activate(string id, CancellationToken cancellationToken) =>
        RunStatusAction(() => _api.ActivateStationAsync(id, cancellationToken), $"Activated {id}.");

    // Runs a status change and always returns to the list, with the API message on failure.
    private async Task<IActionResult> RunStatusAction(Func<Task<StationDto>> command, string success)
    {
        try
        {
            await command();
            TempData["Success"] = success;
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return RedirectToAction(nameof(Index));
    }
}
