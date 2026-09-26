/*
 * File: ApiClient.cs
 * Description: Shared HTTP client for every MVC page. Attach the session JWT; do not encode rules here.
 * Author: Member 1
 * Created: 20/09/2026
 */

using System.Net.Http.Headers;
using System.Text.Json;
using SmartSolar.Web.Models;
using SmartSolar.Web.Services;

namespace SmartSolar.Web.Api;

public partial class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly IWebSession _session;

    // Injects the typed HttpClient (base URL from config) and the session that holds the JWT.
    public ApiClient(HttpClient http, IWebSession session)
    {
        _http = http;
        _session = session;
    }

    // Calls POST /auth/login. No bearer token is sent.
    public Task<LoginResponseDto> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        return SendAsync<LoginResponseDto>(
            HttpMethod.Post,
            "auth/login",
            new { username, password },
            withBearer: false,
            cancellationToken);
    }

    // Calls GET /users (Backoffice staff list).
    public async Task<IReadOnlyList<UserDto>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<UserDto>>(HttpMethod.Get, "users", null, true, cancellationToken);
    }

    // Calls GET /users/{id}.
    public Task<UserDto> GetStaffByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendAsync<UserDto>(HttpMethod.Get, $"users/{Uri.EscapeDataString(id)}", null, true, cancellationToken);
    }

    // Calls POST /users to create a staff account.
    public Task<UserDto> CreateStaffAsync(StaffUserForm form, CancellationToken cancellationToken = default)
    {
        return SendAsync<UserDto>(
            HttpMethod.Post,
            "users",
            new
            {
                form.Username,
                form.Password,
                form.FullName,
                form.Email,
                form.Phone,
                form.Role
            },
            true,
            cancellationToken);
    }

    // Calls PUT /users/{id}. Password is omitted when left blank so the hash is unchanged.
    public Task<UserDto> UpdateStaffAsync(string id, StaffUserForm form, CancellationToken cancellationToken = default)
    {
        object body = string.IsNullOrWhiteSpace(form.Password)
            ? new { form.FullName, form.Email, form.Phone, form.Role }
            : new { form.FullName, form.Email, form.Phone, form.Role, form.Password };

        return SendAsync<UserDto>(HttpMethod.Put, $"users/{Uri.EscapeDataString(id)}", body, true, cancellationToken);
    }

    // Calls POST /users/{id}/deactivate.
    public Task<UserDto> DeactivateStaffAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendAsync<UserDto>(HttpMethod.Post, $"users/{Uri.EscapeDataString(id)}/deactivate", null, true, cancellationToken);
    }

    // Sends JSON to the API and maps non-2xx bodies onto ApiClientException.
    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string relativeUrl,
        object? body,
        bool withBearer,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativeUrl);

        if (withBearer)
        {
            var token = _session.GetJwt();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ApiClientException(StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "You are not signed in.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ApiClientException(
                StatusCodes.Status503ServiceUnavailable,
                "API_UNAVAILABLE",
                "Cannot reach the API. Start SmartSolar.Api on port 5080 and try again.");
        }

        using (response)
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = TryReadError(json);
                throw new ApiClientException(
                    (int)response.StatusCode,
                    error?.Code ?? "ERROR",
                    string.IsNullOrWhiteSpace(error?.Message) ? "The API request failed." : error.Message);
            }

            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (result is null)
            {
                throw new ApiClientException(StatusCodes.Status502BadGateway, "INVALID_RESPONSE", "The API returned an empty body.");
            }

            return result;
        }
    }

    // Parses the contract error JSON when present; returns null if the body is not JSON.
    private static ApiErrorDto? TryReadError(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ApiErrorDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
