/* File: ProsumerForm.cs | Author: DE SILVA R K D H (IT22001252) | Created: 27/09/2026 */
using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class ProsumerForm
{
    [Required, Display(Name = "NIC")]
    public string Nic { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [MinLength(8), DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required, Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    public bool IsEdit { get; set; }
}
