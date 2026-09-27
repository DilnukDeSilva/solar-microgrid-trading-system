/*
 * File: IAuthService.cs
 * Description: Login use-case contract used by AuthController.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IAuthService
{
    // Authenticates by username or NIC and returns a JWT plus the public user.
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    // Creates a Pending prosumer account that Backoffice must activate before login.
    Task<UserDto> RegisterAsync(RegisterProsumerRequestDto request, CancellationToken cancellationToken = default);
}
