/*
 * File: StationService.cs
 * Description: Business rules for listing, reading, updating, activating and finding nearby stations.
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
    private readonly ISlotRepository _slots;

    // Stores the station and slot repositories. Nearby search counts free slots.
    public StationService(IStationRepository stations, ISlotRepository slots)
    {
        _stations = stations;
        _slots = slots;
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

    // Returns Active stations within radiusKm, nearest first, with free slots in the next 7 days.
    public async Task<IReadOnlyList<NearbyStationDto>> NearbyAsync(
        double lat,
        double lng,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        RequireCoordinates(lat, lng);
        if (!double.IsFinite(radiusKm) || radiusKm <= 0 || radiusKm > 100)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Radius must be greater than 0 and at most 100 km.");
        }

        var now = DateTime.UtcNow;
        var stations = await _stations.GetAllAsync(cancellationToken);
        var withinRadius = stations
            .Where(station => station.Status == StationStatuses.Active)
            .Select(station => (Station: station, Distance: DistanceKm(lat, lng, station.Latitude, station.Longitude)))
            .Where(item => item.Distance <= radiusKm)
            .OrderBy(item => item.Distance)
            .ToList();

        var nearby = new List<NearbyStationDto>(withinRadius.Count);
        foreach (var (station, distance) in withinRadius)
        {
            var free = await _slots.GetFreeAsync(station.Id, now, now.AddDays(7), cancellationToken);
            nearby.Add(new NearbyStationDto
            {
                Id = station.Id,
                Name = station.Name,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                CapacityKwh = station.CapacityKwh,
                BatterySlotsTotal = station.BatterySlotsTotal,
                DistanceKm = Math.Round(distance, 2),
                FreeSlots = free.Count
            });
        }

        return nearby;
    }

    // Great-circle distance in kilometres between two GPS points.
    private static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusKm = 6371;
        var phi1 = DegreesToRadians(lat1);
        var phi2 = DegreesToRadians(lat2);
        var deltaPhi = DegreesToRadians(lat2 - lat1);
        var deltaLambda = DegreesToRadians(lng2 - lng1);

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2)
            + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
        return 2 * earthRadiusKm * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // Converts degrees to radians for the haversine formula.
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // Rejects a latitude or longitude outside the GPS ranges.
    private static void RequireCoordinates(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90
            || !double.IsFinite(longitude) || longitude < -180 || longitude > 180)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Latitude must be between -90 and 90, and longitude between -180 and 180.");
        }
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

        RequireCoordinates(latitude, longitude);

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
