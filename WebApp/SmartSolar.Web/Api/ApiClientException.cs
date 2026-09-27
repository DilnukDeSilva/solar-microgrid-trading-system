/*
 * File: ApiClientException.cs
 * Description: Wraps a non-2xx API response so controllers can display the server message.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

namespace SmartSolar.Web.Api;

public class ApiClientException : Exception
{
    public int StatusCode { get; }

    public string Code { get; }

    public string ApiMessage { get; }

    // Stores the HTTP status and contract error fields from the API.
    public ApiClientException(int statusCode, string code, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        ApiMessage = message;
    }
}
