/*
 * File: ReservationSummaryViewModel.cs
 * Description: What the summary page shows after a booking is created, updated, cancelled or approved.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

public class ReservationSummaryViewModel
{
    public ReservationDto Reservation { get; set; } = new();

    public string Done { get; set; } = string.Empty;
}
