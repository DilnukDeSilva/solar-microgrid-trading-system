/*
 * File: UpdateProsumerRequestDto.cs
 * Description: Body for PUT /api/prosumers/{nic} and PUT /api/me. NIC and role are never taken from this body.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class UpdateProsumerRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Username { get; set; }
}
