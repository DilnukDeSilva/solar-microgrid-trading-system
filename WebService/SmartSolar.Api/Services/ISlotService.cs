/*
 * File: ISlotService.cs
 * Description: Slot use-cases. Schedule, battery overlap, booking locks and availability live here.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public interface ISlotService
{
    // Returns one station's slots, earliest start first.
    Task<IReadOnlyList<Slot>> ListByStationAsync(string stationId, CancellationToken cancellationToken = default);

    // Creates an available slot after the schedule and battery rules pass.
    Task<Slot> CreateAsync(string stationId, SaveSlotRequestDto request, CancellationToken cancellationToken = default);

    // Moves a free slot. A slot with an active reservation cannot be moved.
    Task<Slot> UpdateAsync(string slotId, SaveSlotRequestDto request, CancellationToken cancellationToken = default);

    // Deletes a free slot. A slot with an active reservation cannot be deleted.
    Task DeleteAsync(string slotId, CancellationToken cancellationToken = default);

    // Sets the booking flag. A slot with an active reservation cannot be changed either way.
    Task<Slot> SetAvailabilityAsync(string slotId, SlotAvailabilityRequestDto request, CancellationToken cancellationToken = default);
}
