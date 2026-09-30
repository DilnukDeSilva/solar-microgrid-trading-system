/*
 * File: BookingsController.cs
 * Description: Booking monitor, approval and QR verification pages. Rules stay in the API.
 * Author: samudith
 * Created: 29/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
public class BookingsController : Controller
{
    private static readonly TimeSpan SriLankaOffset = new(5, 30, 0);
    private readonly ApiClient _api;

    // Injects the shared API client.
    public BookingsController(ApiClient api)
    {
        _api = api;
    }

    // Shows the filterable booking table. Tabs add upcoming, pending or history bounds.
    [HttpGet]
    public async Task<IActionResult> Index(
        string? tab,
        string? status,
        string? stationId,
        string? nic,
        string? q,
        DateOnly? from,
        DateOnly? to,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        tab = string.IsNullOrWhiteSpace(tab) ? "all" : tab;
        if (tab == "pending")
        {
            status = "Pending";
        }

        var fromUtc = StartOfSriLankaDay(from);
        var toUtc = EndOfSriLankaDay(to);
        if (tab == "upcoming" && fromUtc is null)
        {
            fromUtc = DateTime.UtcNow;
        }

        if (tab == "history" && toUtc is null)
        {
            toUtc = DateTime.UtcNow;
        }

        var model = new BookingMonitorViewModel
        {
            Tab = tab,
            Status = status,
            StationId = stationId,
            Nic = nic,
            Query = q,
            From = from,
            To = to,
            Page = page < 1 ? 1 : page
        };

        try
        {
            model.Stations = await _api.GetStationsAsync(cancellationToken);
            model.Result = await _api.SearchReservationsAsync(status, fromUtc, toUtc, stationId, q, nic, model.Page, cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return View(model);
    }

    // Approves a pending booking through the API, then returns to the monitor.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id, string? tab, CancellationToken cancellationToken)
    {
        try
        {
            await _api.ApproveReservationAsync(id, cancellationToken);
            TempData["Success"] = "Booking approved. The prosumer can show the QR code.";
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return RedirectToAction(nameof(Index), new { tab = string.IsNullOrWhiteSpace(tab) ? "pending" : tab });
    }

    // Checks a pasted QR token and shows the booking the operator is about to finish.
    [HttpPost]
    [Authorize(Roles = RoleNames.GridOperator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(string qrToken, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _api.VerifyQrAsync(qrToken, cancellationToken);
            return View("Verified", reservation);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Marks the verified booking completed.
    [HttpPost]
    [Authorize(Roles = RoleNames.GridOperator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(string id, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _api.CompleteReservationAsync(id, cancellationToken);
            TempData["Success"] = "Booking marked completed.";
            return View("Verified", reservation);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Converts a Sri Lanka calendar date to the UTC instant at local midnight.
    private static DateTime? StartOfSriLankaDay(DateOnly? date)
    {
        if (date is null)
        {
            return null;
        }

        var local = date.Value.ToDateTime(TimeOnly.MinValue);
        return DateTime.SpecifyKind(local - SriLankaOffset, DateTimeKind.Utc);
    }

    // Converts a Sri Lanka calendar date to the UTC instant at local end of day.
    private static DateTime? EndOfSriLankaDay(DateOnly? date)
    {
        if (date is null)
        {
            return null;
        }

        var local = date.Value.ToDateTime(new TimeOnly(23, 59, 59));
        return DateTime.SpecifyKind(local - SriLankaOffset, DateTimeKind.Utc);
    }
}
