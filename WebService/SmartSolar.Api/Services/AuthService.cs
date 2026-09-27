/*
 * File: AuthService.cs
 * Description: Validates credentials, rejects inactive accounts, and issues a JWT.
 * Author: Member 1
 * Created: 20/09/2026
 */

using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly IJwtTokenService _tokens;
    private readonly ProsumerService _prosumers;

    // Wires the repository and crypto helpers used during login.
    public AuthService(IUserRepository users, IPasswordService passwords, IJwtTokenService tokens, ProsumerService prosumers)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _prosumers = prosumers;
    }

    // Registers a Pending account. Activation is deliberately a separate Backoffice action.
    public Task<UserDto> RegisterAsync(RegisterProsumerRequestDto request, CancellationToken cancellationToken = default)
    {
        return _prosumers.CreateAccountAsync(
            request.Nic,
            request.Username,
            request.Password,
            request.FullName,
            request.Email,
            request.Phone,
            UserStatuses.Pending,
            cancellationToken);
    }

    // Authenticates by username or NIC and returns a token plus the public user.
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Password is required.");
        }

        var login = FirstNonEmpty(request.Username, request.Nic);
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Username or NIC is required.");
        }

        var user = await _users.FindByUsernameOrNicAsync(login.Trim(), cancellationToken);
        if (user is null || !_passwords.Verify(user, request.Password))
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, ErrorCodes.InvalidCredentials, "Invalid username or password.");
        }

        if (user.Status is not UserStatuses.Active)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.AccountNotActive,
                "Account is not active. Pending accounts must be activated by Backoffice.");
        }

        var (token, expiresAt) = _tokens.CreateToken(user);
        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = UserDto.From(user)
        };
    }

    // Picks the first non-blank login identifier from the request.
    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
