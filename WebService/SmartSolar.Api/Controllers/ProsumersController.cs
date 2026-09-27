/*
 * File: ProsumersController.cs
 * Description: Staff HTTP endpoints for searching and administering prosumer accounts.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 27/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/prosumers")]
[Produces("application/json")]
public class ProsumersController : ControllerBase
{
    private readonly IProsumerService _prosumers;

    public ProsumersController(IProsumerService prosumers)
    {
        _prosumers = prosumers;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Backoffice + "," + RoleNames.GridOperator)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Search(
        [FromQuery] string? status,
        [FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.SearchAsync(status, query, cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Pending(
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.ListPendingAsync(cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpGet("{nic}")]
    public async Task<ActionResult<UserDto>> Get(
        string nic,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.GetAsync(nic, cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateProsumerRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _prosumers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { nic = created.Nic }, created);
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpPut("{nic}")]
    public async Task<ActionResult<UserDto>> Update(
        string nic,
        [FromBody] UpdateProsumerRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.UpdateAsync(nic, request, cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpPost("{nic}/deactivate")]
    public async Task<ActionResult<UserDto>> Deactivate(
        string nic,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.DeactivateAsync(nic, cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpPost("{nic}/reactivate")]
    public async Task<ActionResult<UserDto>> Reactivate(
        string nic,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.ReactivateAsync(nic, cancellationToken));
    }

    [Authorize(Roles = RoleNames.Backoffice)]
    [HttpPost("{nic}/activate")]
    public async Task<ActionResult<UserDto>> Activate(
        string nic,
        CancellationToken cancellationToken)
    {
        return Ok(await _prosumers.ActivateAsync(nic, cancellationToken));
    }
}