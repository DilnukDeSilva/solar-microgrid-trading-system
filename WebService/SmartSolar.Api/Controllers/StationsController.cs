/*
 * File: StationsController.cs
 * Description: HTTP endpoints for listing, reading and creating stations. Rules stay in StationService.
 * Author: Mohamed Asath (IT22633422)
 * Created: 30/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Models;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/stations")]
[Produces("application/json")]
public class StationsController : ControllerBase
{
    private readonly IStationService _stations;

    // Injects the station service that owns the node rules.
    public StationsController(IStationService stations)
    {
        _stations = stations;
    }

    // Lists stations for any logged-in role.
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<Station>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _stations.ListAsync(cancellationToken));
    }

    // Returns one station for any logged-in role.
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Station>> Get(string id, CancellationToken cancellationToken)
    {
        return Ok(await _stations.GetAsync(id, cancellationToken));
    }

    // Creates a station. Backoffice only.
    [HttpPost]
    [Authorize(Roles = RoleNames.Backoffice)]
    public async Task<ActionResult<Station>> Create(
        [FromBody] SaveStationRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _stations.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
