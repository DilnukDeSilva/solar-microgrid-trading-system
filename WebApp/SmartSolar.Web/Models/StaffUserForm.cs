/*
 * File: StaffUserForm.cs
 * Description: Create/edit form for staff accounts. Required-field checks are UX only; the API enforces rules.
 * Author: Member 1
 * Created: 20/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class StaffUserForm
{
    public string? Id { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Required]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public bool IsEdit { get; set; }
}
