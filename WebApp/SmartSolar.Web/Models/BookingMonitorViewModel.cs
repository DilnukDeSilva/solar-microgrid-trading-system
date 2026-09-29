/*
 * File: BookingMonitorViewModel.cs
 * Description: Filter form and the current page of the booking monitor.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Web.Models;

public class BookingMonitorViewModel
{
    public string Tab { get; set; } = "all";
    public string? Status { get; set; }
    public string? StationId { get; set; }
    public string? Nic { get; set; }
    public string? Query { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public ReservationPageDto Result { get; set; } = new();
}
