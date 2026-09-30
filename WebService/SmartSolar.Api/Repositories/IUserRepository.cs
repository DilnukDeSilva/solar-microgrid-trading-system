/*
 * File: IUserRepository.cs
 * Description: Data-access contract for the Users collection. Other members must reuse this.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public interface IUserRepository
{
    // Returns how many user documents exist.
    Task<long> CountAsync(CancellationToken cancellationToken = default);

    // Loads a user by document id (staff username or prosumer NIC).
    Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    // Resolves a login value against either username or NIC.
    Task<User?> FindByUsernameOrNicAsync(string login, CancellationToken cancellationToken = default);

    // Returns every user document.
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    // Returns Backoffice and GridOperator accounts for the staff Users API.
    Task<IReadOnlyList<User>> GetStaffAsync(CancellationToken cancellationToken = default);

    // Returns prosumer accounts, optionally filtered by status and a free-text search.
    Task<IReadOnlyList<User>> SearchProsumersAsync(string? status, string? query, CancellationToken cancellationToken = default);

    // Inserts a new user document.
    Task InsertAsync(User user, CancellationToken cancellationToken = default);

    // Replaces an existing user document by id.
    Task ReplaceAsync(User user, CancellationToken cancellationToken = default);
}
