/*
 * File: ReservationIndexViewModel.cs
 * Description: Filters and results for the reservations list page.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

public class ReservationIndexViewModel
{
    public string? Status { get; set; }

    public string? Nic { get; set; }

    public string? Q { get; set; }

    public bool IncludePast { get; set; }

    public IReadOnlyList<ReservationDto> Items { get; set; } = [];
}
