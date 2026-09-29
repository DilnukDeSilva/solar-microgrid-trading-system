/*
 * File: HealthResponseDto.cs
 * Description: Payload for GET /health used as a LAN smoke-test probe.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class HealthResponseDto
{
    public string Status { get; set; } = "Healthy";
    public DateTime ServerTime { get; set; }
}
