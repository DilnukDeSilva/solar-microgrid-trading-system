/*
 * File: UserService.cs
 * Description: Staff account rules for Backoffice. Password hashes never leave this layer.
 * Author: Member 1
 * Created: 20/09/2026
 */

using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class UserService : IUserService
{
    public const int MinPasswordLength = 8;

    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    // Injects persistence and hashing used by staff account commands.
    public UserService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    // Returns staff accounts without password hashes.
    public async Task<IReadOnlyList<UserDto>> ListStaffAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.GetStaffAsync(cancellationToken);
        return users.Select(UserDto.From).ToList();
    }

    // Returns one staff user or 404 when the id does not belong to staff.
    public async Task<UserDto> GetStaffByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return UserDto.From(await LoadStaffAsync(id, cancellationToken));
    }

    // Creates an Active staff account after checking username uniqueness and role.
    public async Task<UserDto> CreateStaffAsync(CreateStaffRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateStaffRole(request.Role);
        RequireText(request.Username, "Username is required.");
        RequireText(request.Password, "Password is required.");
        RequirePasswordLength(request.Password);
        RequireText(request.FullName, "Full name is required.");
        RequireEmailFormat(request.Email);

        var username = request.Username.Trim();
        var existing = await _users.FindByUsernameOrNicAsync(username, cancellationToken);
        if (existing is not null)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
        }

        var user = new User
        {
            Id = username,
            Username = username,
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim() ?? string.Empty,
            Phone = request.Phone?.Trim() ?? string.Empty,
            Role = request.Role.Trim(),
            Status = UserStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwords.Hash(user, request.Password);

        try
        {
            await _users.InsertAsync(user, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
        }

        return UserDto.From(user);
    }

    // Updates contact fields and optionally role or password.
    public async Task<UserDto> UpdateStaffAsync(string id, UpdateStaffRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await LoadStaffAsync(id, cancellationToken);
        ValidateStaffRole(request.Role);
        RequireText(request.FullName, "Full name is required.");
        RequireEmailFormat(request.Email);

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim() ?? string.Empty;
        user.Phone = request.Phone?.Trim() ?? string.Empty;
        user.Role = request.Role.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            RequirePasswordLength(request.Password);
            user.PasswordHash = _passwords.Hash(user, request.Password);
        }

        await _users.ReplaceAsync(user, cancellationToken);
        return UserDto.From(user);
    }

    // Marks a staff account Deactivated so they can no longer log in.
    public async Task<UserDto> DeactivateStaffAsync(string id, string? actorId, CancellationToken cancellationToken = default)
    {
        var user = await LoadStaffAsync(id, cancellationToken);
        if (!string.IsNullOrWhiteSpace(actorId) && string.Equals(actorId, user.Id, StringComparison.Ordinal))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "You cannot deactivate your own account.");
        }

        user.Status = UserStatuses.Deactivated;
        await _users.ReplaceAsync(user, cancellationToken);
        return UserDto.From(user);
    }

    // Loads a staff user or throws 404 for missing/prosumer ids.
    private async Task<User> LoadStaffAsync(string id, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id, cancellationToken);
        if (user is null || !IsStaff(user.Role))
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Staff user was not found.");
        }

        return user;
    }

    // Rejects roles that this endpoint must not create (prosumers are Member 2/3).
    private static void ValidateStaffRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role) || !IsStaff(role))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "Role must be Backoffice or GridOperator.");
        }
    }

    // Returns true when the role is a web staff role.
    private static bool IsStaff(string role)
    {
        return role is RoleNames.Backoffice or RoleNames.GridOperator;
    }

    // Throws 400 when a required string is blank.
    private static void RequireText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, message);
        }
    }

    // Rejects a non-blank value that is not an email address. Blank is allowed.
    private static void RequireEmailFormat(string? email)
    {
        var trimmed = email?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return;
        }

        if (!new EmailAddressAttribute().IsValid(trimmed))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Email is not a valid address.");
        }
    }

    // Rejects passwords shorter than the service minimum. The web form is not trusted.
    private static void RequirePasswordLength(string password)
    {
        if (password.Length < MinPasswordLength)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                $"Password must be at least {MinPasswordLength} characters.");
        }
    }
}
