/*
 * File: StationForm.cs
 * Description: Create and edit form for a station. Field rules are enforced by the API.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class StationForm
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Latitude")]
    public double Latitude { get; set; }

    [Display(Name = "Longitude")]
    public double Longitude { get; set; }

    [Display(Name = "Capacity (kWh)")]
    public double CapacityKwh { get; set; }

    [Display(Name = "Battery slots")]
    public int BatterySlotsTotal { get; set; }

    public List<ScheduleRow> Days { get; set; } = NewWeek();

    // Builds Monday-to-Sunday rows, closed until the user ticks Open.
    public static List<ScheduleRow> NewWeek()
    {
        return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }
            .Select(day => new ScheduleRow { DayOfWeek = day })
            .ToList();
    }

    // Copies an API station onto the form and ticks the days that have hours.
    public static StationForm From(StationDto station)
    {
        var form = new StationForm
        {
            Name = station.Name,
            Latitude = station.Latitude,
            Longitude = station.Longitude,
            CapacityKwh = station.CapacityKwh,
            BatterySlotsTotal = station.BatterySlotsTotal
        };

        foreach (var day in form.Days)
        {
            var entry = station.Schedule.FirstOrDefault(item =>
                string.Equals(item.DayOfWeek, day.DayOfWeek, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                continue;
            }

            day.Open = true;
            day.OpenTime = entry.OpenTime;
            day.CloseTime = entry.CloseTime;
        }

        return form;
    }
}

public class ScheduleRow
{
    public string DayOfWeek { get; set; } = string.Empty;

    public bool Open { get; set; }

    public string OpenTime { get; set; } = "08:00";

    public string CloseTime { get; set; } = "18:00";
}
