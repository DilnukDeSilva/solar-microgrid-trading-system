/*
 * File: SlotService.cs
 * Description: Business rules for creating, moving and deleting battery slots.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class SlotService : ISlotService
{
    private readonly ISlotRepository _slots;
    private readonly IStationRepository _stations;

    // Stores the slot and station repositories. Slot rules need the station schedule and battery count.
    public SlotService(ISlotRepository slots, IStationRepository stations)
    {
        _slots = slots;
        _stations = stations;
    }

    // Returns one station's slots, earliest start first.
    public async Task<IReadOnlyList<Slot>> ListByStationAsync(string stationId, CancellationToken cancellationToken = default)
    {
        await LoadStationAsync(stationId, cancellationToken);
        var slots = await _slots.GetByStationAsync(stationId, cancellationToken);
        return slots.OrderBy(slot => slot.StartTime).ToList();
    }

    // Creates an available slot after the schedule and battery rules pass.
    public async Task<Slot> CreateAsync(string stationId, SaveSlotRequestDto request, CancellationToken cancellationToken = default)
    {
        var station = await LoadStationAsync(stationId, cancellationToken);
        if (station.Status != StationStatuses.Active)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.InvalidState,
                "Slots can only be added to an Active station.");
        }

        var existing = await _slots.GetByStationAsync(station.Id, cancellationToken);
        RequireValidSlot(station, request.StartTime, request.EndTime, existing, ignoreSlotId: null);

        var slot = new Slot
        {
            Id = Guid.NewGuid().ToString(),
            StationId = station.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = true
        };

        await _slots.InsertAsync(slot, cancellationToken);
        return slot;
    }

    // Moves a free slot. A slot with an active reservation cannot be moved.
    public async Task<Slot> UpdateAsync(string slotId, SaveSlotRequestDto request, CancellationToken cancellationToken = default)
    {
        var slot = await LoadSlotAsync(slotId, cancellationToken);
        await RejectIfBookedAsync(slot.Id, cancellationToken);

        var station = await LoadStationAsync(slot.StationId, cancellationToken);
        var existing = await _slots.GetByStationAsync(slot.StationId, cancellationToken);
        RequireValidSlot(station, request.StartTime, request.EndTime, existing, slot.Id);

        slot.StartTime = request.StartTime;
        slot.EndTime = request.EndTime;
        await _slots.ReplaceAsync(slot, cancellationToken);
        return slot;
    }

    // Deletes a free slot. A slot with an active reservation cannot be deleted.
    public async Task DeleteAsync(string slotId, CancellationToken cancellationToken = default)
    {
        var slot = await LoadSlotAsync(slotId, cancellationToken);
        await RejectIfBookedAsync(slot.Id, cancellationToken);
        await _slots.DeleteAsync(slot.Id, cancellationToken);
    }

    // Rejects a slot that is in the past, outside the station schedule, or over the battery count.
    private static void RequireValidSlot(
        Station station,
        DateTime start,
        DateTime end,
        IReadOnlyList<Slot> existingSlots,
        string? ignoreSlotId)
    {
        if (start >= end)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Start time must be before end time.");
        }

        if (start <= DateTime.UtcNow)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Start time must be in the future.");
        }

        var localStart = SriLankaClock.ToLocal(start);
        var localEnd = SriLankaClock.ToLocal(end);
        if (localStart.Date != localEnd.Date)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "A slot must start and end on the same day.");
        }

        var entry = (station.Schedule ?? []).FirstOrDefault(item =>
            string.Equals(item.DayOfWeek, localStart.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "The station is closed on that day.");
        }

        var open = TimeOnly.Parse(entry.OpenTime);
        var close = TimeOnly.Parse(entry.CloseTime);
        if (TimeOnly.FromDateTime(localStart) < open || TimeOnly.FromDateTime(localEnd) > close)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "The slot must fall inside the station schedule.");
        }

        var overlapCount = existingSlots.Count(slot =>
            slot.Id != ignoreSlotId
            && slot.StartTime < end
            && start < slot.EndTime);

        if (overlapCount >= station.BatterySlotsTotal)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.SlotOverlap,
                "That time overlaps more slots than this station's battery count allows.");
        }
    }

    // Rejects a change when a Pending or Approved reservation still holds this slot.
    private async Task RejectIfBookedAsync(string slotId, CancellationToken cancellationToken)
    {
        if (await _slots.HasActiveReservationAsync(slotId, cancellationToken))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.SlotTaken,
                "This slot has an active reservation.");
        }
    }

    // Loads a station or throws 404.
    private async Task<Station> LoadStationAsync(string stationId, CancellationToken cancellationToken)
    {
        var station = await _stations.FindByIdAsync(stationId, cancellationToken);
        if (station is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Station was not found.");
        }

        return station;
    }

    // Loads a slot or throws 404.
    private async Task<Slot> LoadSlotAsync(string slotId, CancellationToken cancellationToken)
    {
        var slot = await _slots.FindByIdAsync(slotId, cancellationToken);
        if (slot is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Slot was not found.");
        }

        return slot;
    }
}
