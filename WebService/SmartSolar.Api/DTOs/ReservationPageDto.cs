/*
 * File: ReservationPageDto.cs
 * Description: One page of reservations for the booking monitor and the operator queue.
 * Author: Herath D M S T (IT22639776)
 */

namespace SmartSolar.Api.DTOs;

public class ReservationPageDto
{
    public IReadOnlyList<ReservationDto> Items { get; set; } = Array.Empty<ReservationDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
}
