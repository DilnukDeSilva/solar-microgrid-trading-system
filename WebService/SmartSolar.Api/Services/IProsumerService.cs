/*
 * File: IProsumerService.cs
 * Description: Prosumer profile and Backoffice administration use-cases.
 * Author: Dilnuk De Silva
 * Created: 27/09/2026
 */

using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Services;

public interface IProsumerService
{
    Task<IReadOnlyList<UserDto>> SearchAsync(string? status, string? query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> ListPendingAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetAsync(string nic, CancellationToken cancellationToken = default);
    Task<UserDto> GetMeAsync(string nic, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateProsumerRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(string nic, UpdateProsumerRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateMeAsync(string nic, UpdateProsumerRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> DeactivateAsync(string nic, CancellationToken cancellationToken = default);
    Task<UserDto> ReactivateAsync(string nic, CancellationToken cancellationToken = default);
    Task<UserDto> ActivateAsync(string nic, CancellationToken cancellationToken = default);
}
