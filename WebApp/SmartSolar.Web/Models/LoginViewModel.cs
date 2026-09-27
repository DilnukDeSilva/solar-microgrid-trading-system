/*
 * File: LoginViewModel.cs
 * Description: Login form fields posted to AccountController. The API validates credentials.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolar.Web.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username or NIC")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
