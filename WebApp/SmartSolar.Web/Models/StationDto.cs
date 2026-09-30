/*
 * File: StationDto.cs
 * Description: Station JSON returned by the API, including the weekly schedule.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

namespace SmartSolar.Web.Models;

public class StationDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double CapacityKwh { get; set; }

    public int BatterySlotsTotal { get; set; }

    public List<StationScheduleDto> Schedule { get; set; } = new();

    public string Status { get; set; } = string.Empty;
}

public class StationScheduleDto
{
    public string DayOfWeek { get; set; } = string.Empty;

    public string OpenTime { get; set; } = string.Empty;

    public string CloseTime { get; set; } = string.Empty;
}
