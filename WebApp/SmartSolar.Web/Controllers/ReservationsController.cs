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

    // Lists bookings. Shows upcoming ones unless "include past" is ticked.
    [HttpGet]
    public async Task<IActionResult> Index(ReservationIndexViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            DateTime? from = model.IncludePast ? null : DateTime.UtcNow;
            model.Items = await _api.SearchReservationsAsync(model.Status, model.Nic, model.Q, from, cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return View(model);
    }

    // Shows the booking form. Once a station is picked, it also lists that station's free slots.
    [HttpGet]
    public async Task<IActionResult> Create(ReservationCreateViewModel model, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        await LoadCreateChoicesAsync(model, cancellationToken);
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

        await LoadCreateChoicesAsync(model, cancellationToken);
        return View(model);
    }

    // Shows the current booking and a picker for the new slot.
    [HttpGet]
    public async Task<IActionResult> Edit(ReservationEditViewModel model, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        try
        {
            model.Current = await _api.GetReservationAsync(model.Id, cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }

        model.StationId ??= model.Current.StationId;
        await LoadEditChoicesAsync(model, cancellationToken);
        return View(model);
    }

    // Moves the booking to the chosen slot, then shows the summary page.
    [HttpPost]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirmed(ReservationEditViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.SlotId))
        {
            ModelState.AddModelError(string.Empty, "Please choose a new slot.");
        }
        else
        {
            try
            {
                var updated = await _api.UpdateReservationAsync(model.Id, model.StationId ?? string.Empty, model.SlotId, cancellationToken);
                return RedirectToAction(nameof(Summary), new { id = updated.Id, done = "updated" });
            }
            catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
            {
                ModelState.AddModelError(string.Empty, ex.ApiMessage);
            }
        }

        try
        {
            model.Current = await _api.GetReservationAsync(model.Id, cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }

        await LoadEditChoicesAsync(model, cancellationToken);
        return View(model);
    }

    // Asks staff to confirm the cancellation.
    [HttpGet]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        try
        {
            var current = await _api.GetReservationAsync(id, cancellationToken);
            return View(new ReservationCancelViewModel { Id = id, Current = current });
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Cancels the booking. The API decides if there is still enough notice.
    [HttpPost]
    [ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(ReservationCancelViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var cancelled = await _api.CancelReservationAsync(model.Id, model.Reason, cancellationToken);
            return RedirectToAction(nameof(Summary), new { id = cancelled.Id, done = "cancelled" });
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
        }

        try
        {
            model.Current = await _api.GetReservationAsync(model.Id, cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // Approves a pending booking, then shows the summary page with its QR token.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id, CancellationToken cancellationToken)
    {
        try
        {
            var approved = await _api.ApproveReservationAsync(id, cancellationToken);
            return RedirectToAction(nameof(Summary), new { id = approved.Id, done = "approved" });
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
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
            return RedirectToAction(nameof(Index));
        }
    }

    // Fills the station list and free slots on the create page.
    private async Task LoadCreateChoicesAsync(ReservationCreateViewModel model, CancellationToken cancellationToken)
    {
        (model.Stations, model.Slots, model.SlotsSearched) = await LoadChoicesAsync(model.StationId, model.Date, cancellationToken);
    }

    // Fills the station list and free slots on the edit page.
    private async Task LoadEditChoicesAsync(ReservationEditViewModel model, CancellationToken cancellationToken)
    {
        (model.Stations, model.Slots, model.SlotsSearched) = await LoadChoicesAsync(model.StationId, model.Date, cancellationToken);
    }

    // Gets the bookable stations, and the free slots when a station has been picked.
    private async Task<(IReadOnlyList<BookingStationDto>, IReadOnlyList<AvailableSlotDto>, bool)> LoadChoicesAsync(
        string? stationId,
        DateOnly? date,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BookingStationDto> stations = [];
        IReadOnlyList<AvailableSlotDto> slots = [];
        var searched = false;

        try
        {
            stations = await _api.GetBookableStationsAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(stationId))
            {
                slots = await _api.GetAvailableSlotsAsync(stationId, date, cancellationToken);
                searched = true;
            }
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
        }

        return (stations, slots, searched);
    }
}
