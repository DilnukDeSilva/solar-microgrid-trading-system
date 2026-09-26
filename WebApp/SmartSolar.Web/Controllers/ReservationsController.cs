/*
 * File: ReservationsController.cs
 * Description: Reservation pages for Backoffice and Grid Operators. Calls the API and shows its answer; no booking rules here.
 * Author: Janukshan S (IT22635266)
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
public class ReservationsController : Controller
{
    private readonly ApiClient _api;

    // Injects the shared API client.
    public ReservationsController(ApiClient api)
    {
        _api = api;
    }

    // Shows the booking form. Once a station is picked, it also lists that station's free slots.
    [HttpGet]
    public async Task<IActionResult> Create(ReservationCreateViewModel model, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        await LoadChoicesAsync(model, cancellationToken);
        return View(model);
    }

    // Books the chosen slot for the prosumer, then shows the summary page.
    [HttpPost]
    [ActionName("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed(ReservationCreateViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.SlotId))
        {
            ModelState.AddModelError(string.Empty, "Please choose a slot.");
        }
        else
        {
            try
            {
                var created = await _api.CreateReservationAsync(
                    model.ProsumerNic ?? string.Empty,
                    model.StationId ?? string.Empty,
                    model.SlotId,
                    cancellationToken);

                return RedirectToAction(nameof(Summary), new { id = created.Id, done = "created" });
            }
            catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
            {
                ModelState.AddModelError(string.Empty, ex.ApiMessage);
            }
        }

        await LoadChoicesAsync(model, cancellationToken);
        return View(model);
    }

    // Shows the result of the last action on a booking.
    [HttpGet]
    public async Task<IActionResult> Summary(string id, string? done, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _api.GetReservationAsync(id, cancellationToken);
            return View(new ReservationSummaryViewModel { Reservation = reservation, Done = done ?? string.Empty });
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Create));
        }
    }

    // Fills the station list, and the free slots when a station has been picked.
    private async Task LoadChoicesAsync(ReservationCreateViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            model.Stations = await _api.GetBookableStationsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(model.StationId))
            {
                model.Slots = await _api.GetAvailableSlotsAsync(model.StationId, model.Date, cancellationToken);
                model.SlotsSearched = true;
            }
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
        }
    }
}
