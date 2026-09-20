/*
 * File: ApiUnauthorizedFilter.cs
 * Description: Signs the browser out when the API returns 401 so expired JWTs cannot linger.
 * Author: Member 1
 * Created: 20/09/2026
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartSolar.Web.Api;
using SmartSolar.Web.Services;

namespace SmartSolar.Web.Filters;

public class ApiUnauthorizedFilter : IAsyncExceptionFilter
{
    private readonly IWebSession _session;

    // Injects the session helper used to clear the cookie after a 401.
    public ApiUnauthorizedFilter(IWebSession session)
    {
        _session = session;
    }

    // Redirects to login when a page call fails with 401. Login itself handles its own errors.
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ApiClientException ex || ex.StatusCode != StatusCodes.Status401Unauthorized)
        {
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase)
            && string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _session.SignOutAsync();
        context.Result = new RedirectToActionResult("Login", "Account", new { timedOut = true });
        context.ExceptionHandled = true;
    }
}
