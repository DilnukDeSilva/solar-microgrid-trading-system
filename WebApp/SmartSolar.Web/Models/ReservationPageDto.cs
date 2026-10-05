/*
 * File: ReservationPageDto.cs
 * Description: Paged reservation list returned by GET /reservations.
 * Author: Herath D M S T (IT22639776)
 */

namespace SmartSolar.Web.Models;

public class ReservationPageDto
{
    public List<ReservationDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
}
