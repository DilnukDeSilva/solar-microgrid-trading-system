/*
 * File: ApiClient.Reservations.cs
 * Description: Reservation calls on the shared ApiClient. Kept in its own file to avoid merge conflicts.
 * Author: Janukshan S (IT22635266)
 */

using SmartSolar.Web.Models;

namespace SmartSolar.Web.Api;

public partial class ApiClient
{
    // Calls GET /reservations/bookable-stations.
    public async Task<IReadOnlyList<BookingStationDto>> GetBookableStationsAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<BookingStationDto>>(HttpMethod.Get, "reservations/bookable-stations", null, true, cancellationToken);
    }

    // Calls GET /reservations/available-slots for one station, optionally for one date.
    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(string stationId, DateOnly? date, CancellationToken cancellationToken = default)
    {
        var url = $"reservations/available-slots?stationId={Uri.EscapeDataString(stationId)}";
        if (date is not null)
        {
            url += $"&date={date.Value:yyyy-MM-dd}";
        }

        return await SendAsync<List<AvailableSlotDto>>(HttpMethod.Get, url, null, true, cancellationToken);
    }

    // Calls POST /reservations to book on behalf of a prosumer.
    public Task<ReservationDto> CreateReservationAsync(string prosumerNic, string stationId, string slotId, CancellationToken cancellationToken = default)
    {
        return SendAsync<ReservationDto>(HttpMethod.Post, "reservations", new { prosumerNic, stationId, slotId }, true, cancellationToken);
    }

    // Calls GET /reservations/{id}.
    public Task<ReservationDto> GetReservationAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendAsync<ReservationDto>(HttpMethod.Get, $"reservations/{Uri.EscapeDataString(id)}", null, true, cancellationToken);
    }
}
