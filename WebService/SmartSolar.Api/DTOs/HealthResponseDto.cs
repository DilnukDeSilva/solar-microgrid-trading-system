/*
 * File: HealthResponseDto.cs
 * Description: Payload for GET /health used as a LAN smoke-test probe.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class HealthResponseDto
{
    public string Status { get; set; } = "Healthy";
    public DateTime ServerTime { get; set; }
}
