/*
 * File: UserDto.cs
 * Description: Public user JSON shape returned by the API. Password hashes are never present.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Web.Models;

public class UserDto
{
    public string Id { get; set; } = string.Empty;

    public string? Nic { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
