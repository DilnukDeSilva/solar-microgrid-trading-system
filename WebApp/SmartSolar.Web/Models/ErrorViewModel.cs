/*
 * File: ErrorViewModel.cs
 * Description: Request id shown on the generic error page.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
