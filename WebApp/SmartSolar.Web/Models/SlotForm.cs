/*
 * File: SlotForm.cs
 * Description: Add-slot form. The times are Sri Lanka local and are sent with a +05:30 offset.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class SlotForm
{
    [Display(Name = "Start")]
    public DateTime Start { get; set; }

    [Display(Name = "End")]
    public DateTime End { get; set; }
}

public class SlotsPage
{
    public StationDto Station { get; set; } = new();

    public IReadOnlyList<SlotDto> Slots { get; set; } = new List<SlotDto>();

    public SlotForm Form { get; set; } = new();
}
