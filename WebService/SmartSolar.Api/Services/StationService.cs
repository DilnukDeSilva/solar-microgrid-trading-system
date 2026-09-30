/*
 * File: StationService.cs
 * Description: Business rules for listing, reading, updating and activating stations.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class StationService : IStationService
{
    private readonly IStationRepository _stations;

    // Stores the station repository used by every station use-case.
    public StationService(IStationRepository stations)
    {
        _stations = stations;
    }

    // Returns every station document.
    public Task<IReadOnlyList<Station>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _stations.GetAllAsync(cancellationToken);
    }

    // Returns one station or throws 404 when the id is unknown.
    public async Task<Station> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var station = await _stations.FindByIdAsync(id, cancellationToken);
        if (station is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Station was not found.");
        }

        return station;
    }

    // Creates an Active station after the field rules pass.
    public async Task<Station> CreateAsync(SaveStationRequestDto request, CancellationToken cancellationToken = default)
    {
        var schedule = NormalizeSchedule(request.Schedule);
        RequireValid(request.Name, request.Latitude, request.Longitude, request.CapacityKwh, request.BatterySlotsTotal, schedule);

        var station = new Station
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CapacityKwh = request.CapacityKwh,
            BatterySlotsTotal = request.BatterySlotsTotal,
            Schedule = schedule,
            Status = StationStatuses.Active
        };

        await _stations.InsertAsync(station, cancellationToken);
        return station;
    }

    // Updates station fields. Id and status stay unchanged.
    public async Task<Station> UpdateAsync(string id, SaveStationRequestDto request, CancellationToken cancellationToken = default)
    {
        var station = await GetAsync(id, cancellationToken);
        var schedule = NormalizeSchedule(request.Schedule);
        RequireValid(request.Name, request.Latitude, request.Longitude, request.CapacityKwh, request.BatterySlotsTotal, schedule);

        station.Name = request.Name.Trim();
        station.Latitude = request.Latitude;
        station.Longitude = request.Longitude;
        station.CapacityKwh = request.CapacityKwh;
        station.BatterySlotsTotal = request.BatterySlotsTotal;
        station.Schedule = schedule;

        await _stations.ReplaceAsync(station, cancellationToken);
        return station;
    }

    // Sets a station Inactive, or 409 when a future Pending or Approved reservation exists.
    public async Task<Station> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var station = await GetAsync(id, cancellationToken);
        if (await _stations.HasActiveFutureReservationsAsync(id, DateTime.UtcNow, cancellationToken))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.StationHasReservations,
                "This station has future reservations and cannot be deactivated.");
        }

        station.Status = StationStatuses.Inactive;
        await _stations.ReplaceAsync(station, cancellationToken);
        return station;
    }

    // Sets a station Active again.
    public async Task<Station> ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var station = await GetAsync(id, cancellationToken);
        station.Status = StationStatuses.Active;
        await _stations.ReplaceAsync(station, cancellationToken);
        return station;
    }

    // Rejects a station body that breaks a MEMBER-2 field rule.
    private static void RequireValid(
        string? name,
        double latitude,
        double longitude,
        double capacityKwh,
        int batterySlotsTotal,
        IReadOnlyList<StationScheduleEntry> schedule)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Name is required.");
        }

        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90
            || !double.IsFinite(longitude) || longitude < -180 || longitude > 180)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Latitude must be between -90 and 90, and longitude between -180 and 180.");
        }

        if (!double.IsFinite(capacityKwh) || capacityKwh <= 0 || batterySlotsTotal < 1)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Capacity must be greater than 0 and battery slots must be at least 1.");
        }

        foreach (var entry in schedule)
        {
            if (!TimeOnly.TryParse(entry.OpenTime, out var open)
                || !TimeOnly.TryParse(entry.CloseTime, out var close)
                || open >= close)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    ErrorCodes.ValidationError,
                    "Each schedule entry must open before it closes.");
            }
        }
    }

    // Copies schedule rows so a null list or a later edit of the request cannot change the stored document.
    private static List<StationScheduleEntry> NormalizeSchedule(List<StationScheduleEntry>? schedule)
    {
        if (schedule is null)
        {
            return new List<StationScheduleEntry>();
        }

        if (schedule.Any(entry => entry is null))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Each schedule entry must open before it closes.");
        }

        return schedule.Select(entry => new StationScheduleEntry
        {
            DayOfWeek = entry.DayOfWeek?.Trim() ?? string.Empty,
            OpenTime = entry.OpenTime?.Trim() ?? string.Empty,
            CloseTime = entry.CloseTime?.Trim() ?? string.Empty
        }).ToList();
    }
}
