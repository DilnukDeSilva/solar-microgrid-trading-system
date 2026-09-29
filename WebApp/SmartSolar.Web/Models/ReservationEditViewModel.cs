/*
 * File: ReservationEditViewModel.cs
 * Description: The booking being moved, plus the new station, date and slot picked for it.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

public class ReservationEditViewModel
{
    public string Id { get; set; } = string.Empty;

    public ReservationDto? Current { get; set; }

    public string? StationId { get; set; }

    public DateOnly? Date { get; set; }

    public string? SlotId { get; set; }

    public IReadOnlyList<BookingStationDto> Stations { get; set; } = [];

    public IReadOnlyList<AvailableSlotDto> Slots { get; set; } = [];

    public bool SlotsSearched { get; set; }
}
