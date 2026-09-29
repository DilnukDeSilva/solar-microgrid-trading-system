/*
 * File: ReservationPageDto.cs
 * Description: Paged reservation list returned by GET /reservations.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Web.Models;

public class ReservationPageDto
{
    public List<ReservationDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
}
