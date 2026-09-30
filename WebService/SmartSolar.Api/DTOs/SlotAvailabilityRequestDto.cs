/*
 * File: SlotAvailabilityRequestDto.cs
 * Description: Body for PUT /api/slots/{id}/availability. GridOperator sets the booking flag.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class SlotAvailabilityRequestDto
{
    public bool IsAvailable { get; set; }
}
