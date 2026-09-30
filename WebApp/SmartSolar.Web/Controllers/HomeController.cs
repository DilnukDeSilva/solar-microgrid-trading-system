/*
 * File: HomeController.cs
 * Description: Public landing page and generic error page. Logged-in staff are sent to their role home.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

public class HomeController : Controller
{
    // Shows the public landing page, or redirects signed-in staff to their dashboard.
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome();
        }

        return View();
    }

    // Shows the unhandled-error page.
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Sends Backoffice to Admin and GridOperator to Operations.
    private IActionResult RedirectToRoleHome()
    {
        if (User.IsInRole(RoleNames.Backoffice))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (User.IsInRole(RoleNames.GridOperator))
        {
            return RedirectToAction("Index", "Operations");
        }

        return RedirectToAction("AccessDenied", "Account");
    }
}
