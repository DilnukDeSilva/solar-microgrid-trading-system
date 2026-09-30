/*
 * File: ReservationCreateViewModel.cs
 * Description: Values picked on the "book on behalf of a prosumer" page, plus the lists shown to pick from.
 * Author: Janukshan S (IT22635266)
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class ReservationCreateViewModel
{
    [Display(Name = "Prosumer NIC")]
    public string? ProsumerNic { get; set; }

    [Display(Name = "Station")]
    public string? StationId { get; set; }

    [Display(Name = "Date")]
    public DateOnly? Date { get; set; }

    public string? SlotId { get; set; }

    public IReadOnlyList<BookingStationDto> Stations { get; set; } = [];

    public IReadOnlyList<AvailableSlotDto> Slots { get; set; } = [];

    public bool SlotsSearched { get; set; }
}
