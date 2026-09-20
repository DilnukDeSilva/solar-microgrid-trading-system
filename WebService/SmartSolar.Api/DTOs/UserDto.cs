/*
 * File: UserDto.cs
 * Description: Public user shape. Password hashes are never copied onto this DTO.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.DTOs;

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

    // Maps a MongoDB user document to the public contract shape.
    public static UserDto From(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Nic = user.Nic,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };
    }
}
