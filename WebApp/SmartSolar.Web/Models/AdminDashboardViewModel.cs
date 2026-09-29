/*
 * File: AdminDashboardViewModel.cs
 * Description: Counts shown on the Backoffice home. Numbers come from the API; this model holds display data only.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 29/09/2026
 */

namespace SmartSolar.Web.Models;

public class AdminDashboardViewModel
{
    public int StaffCount { get; set; }

    public int ProsumerCount { get; set; }

    public int ActiveProsumerCount { get; set; }

    public int PendingCount { get; set; }

    public string? LoadError { get; set; }
}
