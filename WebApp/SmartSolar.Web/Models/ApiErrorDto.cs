/*
 * File: ApiErrorDto.cs
 * Description: Contract error body { code, message } parsed from non-2xx API responses.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Web.Models;

public class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
