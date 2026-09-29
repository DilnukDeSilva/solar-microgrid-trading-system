/*
 * File: AdminController.cs
 * Description: Backoffice dashboard. Shows account counts from the API and links to user management.
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

    // Injects the API client used to load live account counts.
    public AdminController(ApiClient api)
    {
        _api = api;
    }

    // Loads staff, prosumer and pending counts, then shows the Backoffice home.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new AdminDashboardViewModel();
        try
        {
            var staff = await _api.GetStaffAsync(cancellationToken);
            var prosumers = await _api.GetProsumersAsync(null, null, cancellationToken);
            var pending = await _api.GetPendingProsumersAsync(cancellationToken);
            model.StaffCount = staff.Count;
            model.ProsumerCount = prosumers.Count;
            model.ActiveProsumerCount = prosumers.Count(user => user.Status == "Active");
            model.PendingCount = pending.Count;
        }
        catch (ApiClientException ex)
        {
            model.LoadError = ex.ApiMessage;
        }

        return View(model);
    }
}
