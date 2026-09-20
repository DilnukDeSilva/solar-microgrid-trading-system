/*
 * File: ApiException.cs
 * Description: Thrown by the service layer so the global handler can return a contract error body.
 * Author: Member 1
 * Created: 20/09/2026
 */

namespace SmartSolar.Api.Common;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    // Captures an HTTP status, machine-readable code and human-readable message.
    public ApiException(int statusCode, string code, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }
}
