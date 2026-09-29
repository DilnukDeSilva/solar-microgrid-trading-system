/*
 * File: AdminController.cs
 * Description: Backoffice dashboard. Links only; Member 2 owns stations, slots and reservations UI.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice)]
public class AdminController : Controller
{
    private readonly ApiClient _api;

    // Injects the shared API client used for the live Home figures.
    public AdminController(ApiClient api)
    {
        _api = api;
    }

    // Shows the Backoffice home with live operations counts and a link to user management.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await LoadDashboardAsync(cancellationToken));
    }

    // Loads the operations dashboard, or an empty one when the API call fails.
    private async Task<OperationsDashboardDto> LoadDashboardAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _api.GetOperationsDashboardAsync(cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return new OperationsDashboardDto();
        }
    }
}
