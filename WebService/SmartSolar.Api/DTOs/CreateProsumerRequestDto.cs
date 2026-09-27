/*
 * File: CreateProsumerRequestDto.cs
 * Description: Body for POST /api/prosumers. Backoffice creates an Active prosumer.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class CreateProsumerRequestDto
{
    public string Nic { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
}
