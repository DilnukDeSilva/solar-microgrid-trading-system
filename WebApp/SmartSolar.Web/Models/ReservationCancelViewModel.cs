/*
 * File: ReservationCancelViewModel.cs
 * Description: The booking to cancel and the optional reason typed by staff.
 * Author: Janukshan S (IT22635266)
 */

namespace SmartSolar.Web.Models;

public class ReservationCancelViewModel
{
    public string Id { get; set; } = string.Empty;

    public ReservationDto? Current { get; set; }

    public string? Reason { get; set; }
}
