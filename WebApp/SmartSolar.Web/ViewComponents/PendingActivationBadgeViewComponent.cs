/* File: PendingActivationBadgeViewComponent.cs | Author: Dilnuk De Silva | Created: 27/09/2026 */
using Microsoft.AspNetCore.Mvc;
using SmartSolar.Web.Api;

namespace SmartSolar.Web.ViewComponents;

public class PendingActivationBadgeViewComponent : ViewComponent
{
    private readonly ApiClient _api;
    public PendingActivationBadgeViewComponent(ApiClient api) { _api = api; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try { return View(await _api.GetPendingProsumersAsync(HttpContext.RequestAborted)); }
        catch (ApiClientException) { return View(Array.Empty<Models.UserDto>()); }
    }
}
