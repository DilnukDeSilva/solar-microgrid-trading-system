/*
 * File: AdminController.cs
 * Description: Backoffice dashboard. Links only; Member 2 owns stations, slots and reservations UI.
 * Author: Member 1
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Common;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice)]
public class AdminController : Controller
{
    // Shows the Backoffice home with links to user management.
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
