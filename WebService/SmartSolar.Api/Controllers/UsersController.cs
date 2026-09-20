/*
 * File: UsersController.cs
 * Description: Backoffice-only staff account endpoints. Thin HTTP adapter over UserService.
 * Author: Member 1
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;
using SmartSolar.Api.Services;

namespace SmartSolar.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.Backoffice)]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    // Injects the user service that maps entities to public DTOs.
    public UsersController(IUserService users)
    {
        _users = users;
    }

    // Lists staff accounts. Other roles receive 403.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _users.ListStaffAsync(cancellationToken));
    }

    // Returns one staff user by id.
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(string id, CancellationToken cancellationToken)
    {
        return Ok(await _users.GetStaffByIdAsync(id, cancellationToken));
    }

    // Creates a Backoffice or GridOperator account.
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateStaffRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _users.CreateStaffAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Updates a staff account.
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UpdateStaffRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _users.UpdateStaffAsync(id, request, cancellationToken));
    }

    // Deactivates a staff account so they can no longer log in.
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Deactivate(string id, CancellationToken cancellationToken)
    {
        var actorId = User.FindFirst("sub")?.Value;
        return Ok(await _users.DeactivateStaffAsync(id, actorId, cancellationToken));
    }
}
