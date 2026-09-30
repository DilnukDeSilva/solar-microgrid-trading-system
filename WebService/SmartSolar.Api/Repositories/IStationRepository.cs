/*
 * File: IStationRepository.cs
 * Description: Data-access contract for SolarStationInfo. Ready for Member 2 to extend.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using SmartSolar.Api.Models;

namespace SmartSolar.Api.Repositories;

public interface IStationRepository
{
    // Returns every station document.
    Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default);

    // Loads one station by id.
    Task<Station?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    // Inserts a new station document.
    Task InsertAsync(Station station, CancellationToken cancellationToken = default);

    // Replaces an existing station document by id.
    Task ReplaceAsync(Station station, CancellationToken cancellationToken = default);
}
