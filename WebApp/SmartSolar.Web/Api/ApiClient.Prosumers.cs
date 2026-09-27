/* File: ApiClient.Prosumers.cs | Author: Dilnuk De Silva | Created: 27/09/2026 */
using SmartSolar.Web.Models;

namespace SmartSolar.Web.Api;

public partial class ApiClient
{
    public async Task<IReadOnlyList<UserDto>> GetProsumersAsync(string? status, string? query, CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) parameters.Add("status=" + Uri.EscapeDataString(status));
        if (!string.IsNullOrWhiteSpace(query)) parameters.Add("q=" + Uri.EscapeDataString(query));
        var suffix = parameters.Count == 0 ? string.Empty : "?" + string.Join("&", parameters);
        return await SendAsync<List<UserDto>>(HttpMethod.Get, "prosumers" + suffix, null, true, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetPendingProsumersAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<UserDto>>(HttpMethod.Get, "prosumers/pending", null, true, cancellationToken);
    }

    public Task<UserDto> GetProsumerAsync(string nic, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(HttpMethod.Get, $"prosumers/{Uri.EscapeDataString(nic)}", null, true, cancellationToken);

    public Task<UserDto> CreateProsumerAsync(ProsumerForm form, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(HttpMethod.Post, "prosumers", new
        {
            form.Nic, form.Username, form.Password, form.FullName, form.Email, form.Phone
        }, true, cancellationToken);

    public Task<UserDto> UpdateProsumerAsync(string nic, ProsumerForm form, CancellationToken cancellationToken = default)
    {
        object body = string.IsNullOrWhiteSpace(form.Password)
            ? new { form.Username, form.FullName, form.Email, form.Phone }
            : new { form.Username, form.Password, form.FullName, form.Email, form.Phone };
        return SendAsync<UserDto>(HttpMethod.Put, $"prosumers/{Uri.EscapeDataString(nic)}", body, true, cancellationToken);
    }

    public Task<UserDto> DeactivateProsumerAsync(string nic, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(HttpMethod.Post, $"prosumers/{Uri.EscapeDataString(nic)}/deactivate", null, true, cancellationToken);

    public Task<UserDto> ReactivateProsumerAsync(string nic, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(HttpMethod.Post, $"prosumers/{Uri.EscapeDataString(nic)}/reactivate", null, true, cancellationToken);

    public Task<UserDto> ActivateProsumerAsync(string nic, CancellationToken cancellationToken = default) =>
        SendAsync<UserDto>(HttpMethod.Post, $"prosumers/{Uri.EscapeDataString(nic)}/activate", null, true, cancellationToken);
}
