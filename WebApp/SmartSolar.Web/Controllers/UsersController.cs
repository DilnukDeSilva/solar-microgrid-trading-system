/*
 * File: UsersController.cs
 * Description: Backoffice user-management pages. Controllers only call ApiClient and display results.
 * Author: Member 1
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;
using SmartSolar.Web.Common;
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Controllers;

[Authorize(Roles = RoleNames.Backoffice)]
public class UsersController : Controller
{
    private readonly ApiClient _api;

    // Injects the shared API client used by every staff-user page.
    public UsersController(ApiClient api)
    {
        _api = api;
    }

    // Lists staff accounts from GET /api/users.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _api.GetStaffAsync(cancellationToken);
            return View(users);
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
            return View(Array.Empty<UserDto>());
        }
    }

    // Shows the create form.
    [HttpGet]
    public IActionResult Create()
    {
        return View(new StaffUserForm { Role = RoleNames.GridOperator });
    }

    // Posts a new staff account to POST /api/users.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffUserForm form, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(form.Password))
        {
            ModelState.AddModelError(nameof(form.Password), "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await _api.CreateStaffAsync(form, cancellationToken);
            TempData["Success"] = $"Created staff account {form.Username}.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
            return View(form);
        }
    }

    // Loads one staff account into the edit form.
    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _api.GetStaffByIdAsync(id, cancellationToken);
            return View(ToForm(user, isEdit: true));
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Posts updates to PUT /api/users/{id}.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, StaffUserForm form, CancellationToken cancellationToken)
    {
        form.IsEdit = true;
        form.Id = id;
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        try
        {
            await _api.UpdateStaffAsync(id, form, cancellationToken);
            TempData["Success"] = $"Updated {form.Username}.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ApiMessage);
            return View(form);
        }
    }

    // Confirms deactivation before calling the API.
    [HttpGet]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _api.GetStaffByIdAsync(id, cancellationToken);
            return View(user);
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
            return RedirectToAction(nameof(Index));
        }
    }

    // Posts deactivation to POST /api/users/{id}/deactivate.
    [HttpPost]
    [ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _api.DeactivateStaffAsync(id, cancellationToken);
            TempData["Success"] = $"{user.Username} is now Deactivated.";
        }
        catch (ApiClientException ex)
        {
            TempData["Error"] = ex.ApiMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    // Copies API user fields onto the edit form. Mapping only; no rules.
    private static StaffUserForm ToForm(UserDto user, bool isEdit)
    {
        return new StaffUserForm
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsEdit = isEdit
        };
    }
}
