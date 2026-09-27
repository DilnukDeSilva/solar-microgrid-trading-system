/*
 * File: LoginResponseDto.cs
 * Description: Successful login payload: JWT, expiry and the public user object.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}
