/*
 * File: IStationService.cs
 * Description: Station use-cases. All node rules live here, not in the controller.
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
}
