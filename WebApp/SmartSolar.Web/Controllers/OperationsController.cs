/*
 * File: OperationsController.cs
 * Description: GridOperator home. Member 2 will hang node and reservation pages off this layout.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Common;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.GridOperator)]
public class OperationsController : Controller
{
    // Shows the GridOperator home. No operational rules are evaluated here.
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
