/*
 * File: ProsumerService.cs
 * Description: Business rules for registration, profiles and staff-managed prosumer accounts.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

using MongoDB.Driver;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Repositories;

namespace SmartSolar.Api.Services;

public class ProsumerService : IProsumerService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    public ProsumerService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<IReadOnlyList<UserDto>> SearchAsync(string? status, string? query, CancellationToken cancellationToken = default)
    {
        ValidateStatusFilter(status);
        var users = await _users.SearchProsumersAsync(status, query, cancellationToken);
        return users.Select(UserDto.From).ToList();
    }

    public Task<IReadOnlyList<UserDto>> ListPendingAsync(CancellationToken cancellationToken = default)
    {
        return SearchAsync(UserStatuses.Pending, null, cancellationToken);
    }

    public async Task<UserDto> GetAsync(string nic, CancellationToken cancellationToken = default)
    {
        return UserDto.From(await LoadProsumerAsync(nic, cancellationToken));
    }

    public Task<UserDto> GetMeAsync(string nic, CancellationToken cancellationToken = default)
    {
        return GetAsync(nic, cancellationToken);
    }

    public Task<UserDto> CreateAsync(CreateProsumerRequestDto request, CancellationToken cancellationToken = default)
    {
        return CreateAccountAsync(
            request.Nic,
            request.Username,
            request.Password,
            request.FullName,
            request.Email,
            request.Phone,
            UserStatuses.Active,
            cancellationToken);
    }

    public Task<UserDto> UpdateAsync(string nic, UpdateProsumerRequestDto request, CancellationToken cancellationToken = default)
    {
        return UpdateAccountAsync(nic, request, cancellationToken);
    }

    public Task<UserDto> UpdateMeAsync(string nic, UpdateProsumerRequestDto request, CancellationToken cancellationToken = default)
    {
        return UpdateAccountAsync(nic, request, cancellationToken);
    }

    public Task<UserDto> DeactivateAsync(string nic, CancellationToken cancellationToken = default)
    {
        return SetStatusAsync(nic, UserStatuses.Deactivated, requiredCurrentStatus: null, cancellationToken);
    }

    public Task<UserDto> ReactivateAsync(string nic, CancellationToken cancellationToken = default)
    {
        return SetStatusAsync(nic, UserStatuses.Active, UserStatuses.Deactivated, cancellationToken);
    }

    public Task<UserDto> ActivateAsync(string nic, CancellationToken cancellationToken = default)
    {
        return SetStatusAsync(nic, UserStatuses.Active, UserStatuses.Pending, cancellationToken);
    }

    internal async Task<UserDto> CreateAccountAsync(
        string? nic,
        string? username,
        string? password,
        string? fullName,
        string? email,
        string? phone,
        string status,
        CancellationToken cancellationToken)
    {
        NicRules.RequireValid(nic);
        AccountRules.RequireText(username, "Username is required.");
        AccountRules.RequireText(password, "Password is required.");
        AccountRules.RequirePasswordLength(password!);
        AccountRules.RequireText(fullName, "Full name is required.");
        AccountRules.RequireEmailFormat(email);
        AccountRules.RequirePhoneFormat(phone);

        var normalizedNic = NicRules.Normalize(nic!);
        var normalizedUsername = username!.Trim();

        if (await _users.FindByIdAsync(normalizedNic, cancellationToken) is not null)
        {
            throw NicExists();
        }

        var existingLogin = await _users.FindByUsernameOrNicAsync(normalizedUsername, cancellationToken);
        if (existingLogin is not null)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
        }

        var user = new User
        {
            Id = normalizedNic,
            Nic = normalizedNic,
            Username = normalizedUsername,
            FullName = fullName!.Trim(),
            Email = email?.Trim() ?? string.Empty,
            Phone = NormalizePhone(phone),
            Role = RoleNames.Prosumer,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwords.Hash(user, password!);

        try
        {
            await _users.InsertAsync(user, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // A concurrent request may win after the pre-check. Re-read to return a stable contract code.
            if (await _users.FindByIdAsync(normalizedNic, cancellationToken) is not null)
            {
                throw NicExists();
            }

            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
        }

        return UserDto.From(user);
    }

    private async Task<UserDto> UpdateAccountAsync(string nic, UpdateProsumerRequestDto request, CancellationToken cancellationToken)
    {
        var user = await LoadProsumerAsync(nic, cancellationToken);
        AccountRules.RequireText(request.FullName, "Full name is required.");
        AccountRules.RequireEmailFormat(request.Email);
        AccountRules.RequirePhoneFormat(request.Phone);

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var username = request.Username.Trim();
            var existing = await _users.FindByUsernameOrNicAsync(username, cancellationToken);
            if (existing is not null && !string.Equals(existing.Id, user.Id, StringComparison.Ordinal))
            {
                throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
            }

            user.Username = username;
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim() ?? string.Empty;
        user.Phone = NormalizePhone(request.Phone);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            AccountRules.RequirePasswordLength(request.Password);
            user.PasswordHash = _passwords.Hash(user, request.Password);
        }

        try
        {
            await _users.ReplaceAsync(user, cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ApiException(StatusCodes.Status409Conflict, ErrorCodes.UsernameExists, "That username is already taken.");
        }

        return UserDto.From(user);
    }

    private async Task<UserDto> SetStatusAsync(
        string nic,
        string nextStatus,
        string? requiredCurrentStatus,
        CancellationToken cancellationToken)
    {
        var user = await LoadProsumerAsync(nic, cancellationToken);
        if (requiredCurrentStatus is not null && user.Status != requiredCurrentStatus)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                ErrorCodes.InvalidState,
                $"Only {requiredCurrentStatus} prosumers can be changed to {nextStatus}.");
        }

        user.Status = nextStatus;
        await _users.ReplaceAsync(user, cancellationToken);
        return UserDto.From(user);
    }

    private async Task<User> LoadProsumerAsync(string nic, CancellationToken cancellationToken)
    {
        var normalizedNic = NicRules.Normalize(nic);
        var user = await _users.FindByIdAsync(normalizedNic, cancellationToken);
        if (user is null || user.Role != RoleNames.Prosumer)
        {
            throw new ApiException(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "Prosumer was not found.");
        }

        return user;
    }

    private static void ValidateStatusFilter(string? status)
    {
        if (!string.IsNullOrWhiteSpace(status)
            && status is not UserStatuses.Pending and not UserStatuses.Active and not UserStatuses.Deactivated)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, "Status is not valid.");
        }
    }

    private static string NormalizePhone(string? phone)
    {
        return phone?.Trim().Replace(" ", string.Empty) ?? string.Empty;
    }

    private static ApiException NicExists()
    {
        return new ApiException(StatusCodes.Status409Conflict, ErrorCodes.NicExists, "A prosumer with that NIC already exists.");
    }
}
