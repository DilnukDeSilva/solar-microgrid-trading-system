/*
 * File: ApiErrorDto.cs
 * Description: Error body for every non-2xx response: { code, message }.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Parameterless constructor required for JSON serialization.
    public ApiErrorDto()
    {
    }

    // Builds the contract error object used by middleware and JWT events.
    public ApiErrorDto(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
