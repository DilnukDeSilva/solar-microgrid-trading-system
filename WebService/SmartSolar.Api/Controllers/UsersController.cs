/*
 * File: UsersController.cs
 * Description: Backoffice-only user reads. Used to prove 401/403/200 role checks.
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

    // Lists all users. Backoffice only; other roles receive 403.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _users.ListAsync(cancellationToken));
    }

    // Returns one user by id. Missing ids become 404 via ApiException.
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(string id, CancellationToken cancellationToken)
    {
        return Ok(await _users.GetByIdAsync(id, cancellationToken));
    }
}
