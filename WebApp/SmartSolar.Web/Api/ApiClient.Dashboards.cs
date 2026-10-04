/*
 * File: ApiClient.Dashboards.cs
 * Description: Booking list, dashboard, approval and QR calls. Kept separate to avoid merge conflicts.
 * Author: Herath D M S T (IT22639776)
 */

using SmartSolar.Web.Models;

namespace SmartSolar.Web.Api;

public partial class ApiClient
{
    // Calls GET /reservations with the combined filters.
    public Task<ReservationPageDto> SearchReservationsAsync(
        string? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? stationId,
        string? query,
        string? nic,
        int page,
        CancellationToken cancellationToken = default)
    {
        var url = "reservations?page=" + (page < 1 ? 1 : page);
        url += Append("status", status);
        url += Append("stationId", stationId);
        url += Append("q", query);
        url += Append("nic", nic);
        if (fromUtc is not null)
        {
            url += "&from=" + Uri.EscapeDataString(fromUtc.Value.ToUniversalTime().ToString("o"));
        }

        if (toUtc is not null)
        {
            url += "&to=" + Uri.EscapeDataString(toUtc.Value.ToUniversalTime().ToString("o"));
        }

        return SendAsync<ReservationPageDto>(HttpMethod.Get, url, null, true, cancellationToken);
    }

    // Calls GET /dashboard/operations.
    public Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync<OperationsDashboardDto>(HttpMethod.Get, "dashboard/operations", null, true, cancellationToken);
    }

    // Calls POST /reservations/verify-qr.
    public Task<ReservationDto> VerifyQrAsync(string qrToken, CancellationToken cancellationToken = default)
    {
        return SendAsync<ReservationDto>(HttpMethod.Post, "reservations/verify-qr", new { qrToken }, true, cancellationToken);
    }

    // Calls POST /reservations/{id}/complete.
    public Task<ReservationDto> CompleteReservationAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendAsync<ReservationDto>(HttpMethod.Post, $"reservations/{Uri.EscapeDataString(id)}/complete", null, true, cancellationToken);
    }

    // Adds one query parameter when the value is not blank.
    private static string Append(string name, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : "&" + name + "=" + Uri.EscapeDataString(value.Trim());
    }
}
