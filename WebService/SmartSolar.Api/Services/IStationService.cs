/*
 * File: IStationService.cs
 * Description: Station use-cases. All node rules, including deactivation, live here.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public interface IStationService
{
    // Returns every station document.
    Task<IReadOnlyList<Station>> ListAsync(CancellationToken cancellationToken = default);

    // Returns one station or throws 404 when the id is unknown.
    Task<Station> GetAsync(string id, CancellationToken cancellationToken = default);

    // Creates an Active station after the field rules pass.
    Task<Station> CreateAsync(SaveStationRequestDto request, CancellationToken cancellationToken = default);

    // Updates station fields. Id and status stay unchanged.
    Task<Station> UpdateAsync(string id, SaveStationRequestDto request, CancellationToken cancellationToken = default);

    // Sets a station Inactive, or 409 when a future Pending or Approved reservation exists.
    Task<Station> DeactivateAsync(string id, CancellationToken cancellationToken = default);

    // Sets a station Active again.
    Task<Station> ActivateAsync(string id, CancellationToken cancellationToken = default);
}
