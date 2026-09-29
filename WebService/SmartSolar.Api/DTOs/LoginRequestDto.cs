/*
 * File: LoginRequestDto.cs
 * Description: Login body: username or NIC plus password, matching API-CONTRACT.md.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class LoginRequestDto
{
    public string? Username { get; set; }
    public string? Nic { get; set; }
    public string Password { get; set; } = string.Empty;
}
