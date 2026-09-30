/*
 * File: ApiClient.Stations.cs
 * Description: Station calls for the MVC pages. The JWT is attached; rules stay on the API.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Web.Models;

namespace SmartSolar.Web.Api;

public partial class ApiClient
{
    // Calls GET /stations.
    public async Task<IReadOnlyList<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<StationDto>>(HttpMethod.Get, "stations", null, true, cancellationToken);
    }

    // Calls GET /stations/{id}.
    public Task<StationDto> GetStationAsync(string id, CancellationToken cancellationToken = default) =>
        SendAsync<StationDto>(HttpMethod.Get, $"stations/{Uri.EscapeDataString(id)}", null, true, cancellationToken);

    // Calls POST /stations/{id}/deactivate.
    public Task<StationDto> DeactivateStationAsync(string id, CancellationToken cancellationToken = default) =>
        SendAsync<StationDto>(HttpMethod.Post, $"stations/{Uri.EscapeDataString(id)}/deactivate", null, true, cancellationToken);

    // Calls POST /stations/{id}/activate.
    public Task<StationDto> ActivateStationAsync(string id, CancellationToken cancellationToken = default) =>
        SendAsync<StationDto>(HttpMethod.Post, $"stations/{Uri.EscapeDataString(id)}/activate", null, true, cancellationToken);

    // Calls POST /stations. Closed days are omitted from the schedule.
    public Task<StationDto> CreateStationAsync(StationForm form, CancellationToken cancellationToken = default) =>
        SendAsync<StationDto>(HttpMethod.Post, "stations", StationBody(form), true, cancellationToken);

    // Calls PUT /stations/{id}. Closed days are omitted from the schedule.
    public Task<StationDto> UpdateStationAsync(string id, StationForm form, CancellationToken cancellationToken = default) =>
        SendAsync<StationDto>(HttpMethod.Put, $"stations/{Uri.EscapeDataString(id)}", StationBody(form), true, cancellationToken);

    // Calls GET /stations/{id}/slots.
    public async Task<IReadOnlyList<SlotDto>> GetSlotsAsync(string stationId, CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<SlotDto>>(HttpMethod.Get, $"stations/{Uri.EscapeDataString(stationId)}/slots", null, true, cancellationToken);
    }

    // Calls POST /stations/{id}/slots. Local form times are sent as +05:30.
    public Task<SlotDto> CreateSlotAsync(string stationId, SlotForm form, CancellationToken cancellationToken = default)
    {
        var offset = TimeSpan.FromHours(5.5);
        return SendAsync<SlotDto>(
            HttpMethod.Post,
            $"stations/{Uri.EscapeDataString(stationId)}/slots",
            new
            {
                StartTime = new DateTimeOffset(form.Start, offset),
                EndTime = new DateTimeOffset(form.End, offset)
            },
            true,
            cancellationToken);
    }

    // Calls DELETE /slots/{id}. A successful delete has an empty body.
    public Task DeleteSlotAsync(string id, CancellationToken cancellationToken = default) =>
        SendAsync<object>(HttpMethod.Delete, $"slots/{Uri.EscapeDataString(id)}", null, true, cancellationToken, allowEmptyBody: true);

    // Calls PUT /slots/{id}/availability.
    public Task<SlotDto> SetSlotAvailabilityAsync(string id, bool isAvailable, CancellationToken cancellationToken = default) =>
        SendAsync<SlotDto>(HttpMethod.Put, $"slots/{Uri.EscapeDataString(id)}/availability", new { isAvailable }, true, cancellationToken);

    // Builds the station JSON. Only days marked open are included.
    private static object StationBody(StationForm form) => new
    {
        form.Name,
        form.Latitude,
        form.Longitude,
        form.CapacityKwh,
        form.BatterySlotsTotal,
        Schedule = form.Days.Where(day => day.Open).Select(day => new { day.DayOfWeek, day.OpenTime, day.CloseTime })
    };
}
