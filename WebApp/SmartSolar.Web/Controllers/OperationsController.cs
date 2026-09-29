/*
 * File: OperationsController.cs
 * Description: GridOperator home. Member 2 will hang node and reservation pages off this layout.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.GridOperator)]
public class OperationsController : Controller
{
    private readonly ApiClient _api;

    // Injects the shared API client used for the live Home figures.
    public OperationsController(ApiClient api)
    {
        _api = api;
    }

    // Shows the Grid Operator home with live counts. No operational rules are evaluated here.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await _api.GetOperationsDashboardAsync(cancellationToken));
        }
        catch (ApiClientException ex) when (ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            TempData["Error"] = ex.ApiMessage;
            return View(new OperationsDashboardDto());
        }
    }
}
