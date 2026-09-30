/*
 * File: UpdateStaffRequestDto.cs
 * Description: Body for PUT /api/users/{id}. Password is optional; omit it to keep the hash.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class UpdateStaffRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Password { get; set; }
}
