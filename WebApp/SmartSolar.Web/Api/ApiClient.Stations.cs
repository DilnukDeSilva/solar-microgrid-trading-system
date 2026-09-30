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
