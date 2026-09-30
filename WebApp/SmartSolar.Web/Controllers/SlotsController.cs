/*
 * File: SlotsController.cs
 * Description: Staff MVC page for one station's slots. Schedule and booking rules stay on the API.
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
public class SlotsController : Controller
{
    private readonly ApiClient _api;

    // Injects the API client that attaches the session JWT.
    public SlotsController(ApiClient api)
    {
        _api = api;
    }

    // Shows one station and its slots.
    [HttpGet]
    public async Task<IActionResult> Index(string stationId, CancellationToken cancellationToken)
    {
        try
        {
            return View(await LoadPageAsync(stationId, new SlotForm(), cancellationToken));
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction("Index", "Stations");
        }
    }

    // Adds a slot. The API's validation message is shown on the form.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string stationId, SlotForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await LoadPageAsync(stationId, form, cancellationToken));
        }

        try
        {
            await _api.CreateSlotAsync(stationId, form, cancellationToken);
            TempData["Success"] = "Slot added.";
            return RedirectToAction(nameof(Index), new { stationId });
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
            return View("Index", await LoadPageAsync(stationId, form, cancellationToken));
        }
    }

    // Deletes a slot. A booked slot's 409 is shown as a banner.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string stationId, CancellationToken cancellationToken)
    {
        try
        {
            await _api.DeleteSlotAsync(id, cancellationToken);
            TempData["Success"] = "Slot deleted.";
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return RedirectToAction(nameof(Index), new { stationId });
    }

    // Opens or closes a free slot. GridOperator only.
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleNames.GridOperator)]
    public async Task<IActionResult> Availability(string id, string stationId, bool isAvailable, CancellationToken cancellationToken)
    {
        try
        {
            await _api.SetSlotAvailabilityAsync(id, isAvailable, cancellationToken);
            TempData["Success"] = isAvailable ? "Slot opened." : "Slot closed.";
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return RedirectToAction(nameof(Index), new { stationId });
    }

    // Loads the station and its slots for the page.
    private async Task<SlotsPage> LoadPageAsync(string stationId, SlotForm form, CancellationToken cancellationToken)
    {
        var station = await _api.GetStationAsync(stationId, cancellationToken);
        var slots = await _api.GetSlotsAsync(stationId, cancellationToken);
        return new SlotsPage { Station = station, Slots = slots, Form = form };
    }
}
